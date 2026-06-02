/**
 * K6 stress blueprint — Predictive Middleware Framework (Part 4)
 *
 * Traffic shapes:
 *   Phase 1: Nominal baseline (10 RPS, 30s)
 *   Phase 2: Cliff burst (800 RPS target, 15s) — both critical + non-critical endpoints
 *   Phase 3: Cool-down (5 RPS) — verify SSA returns posture to Nominal
 *
 * Run:
 *   k6 run load-tests/k6/predictive-middleware-stress.js
 *   k6 run -e BASE_URL=http://localhost:5080 load-tests/k6/predictive-middleware-stress.js
 */
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5080';
const CHECKOUT_URL = `${BASE_URL}/api/v1/payment/checkout`;
const PROMOTIONS_URL = `${BASE_URL}/api/v1/promotions/ads`;

const checkoutOk = new Rate('checkout_ok');
const promotionsOk = new Rate('promotions_ok');
const throttled429 = new Counter('throttled_429');
const checkoutDuration = new Trend('checkout_duration', true);
const promotionsDuration = new Trend('promotions_duration', true);

export const options = {
  scenarios: {
    // Phase 1 — steady nominal baseline (10 RPS total ≈ 5 per endpoint)
    baseline: {
      executor: 'constant-arrival-rate',
      rate: 10,
      timeUnit: '1s',
      duration: '30s',
      preAllocatedVUs: 20,
      maxVUs: 50,
      startTime: '0s',
      exec: 'mixedTraffic',
      tags: { phase: 'baseline' },
    },
    // Phase 2 — vertical cliff burst (~800 RPS aggregate)
    spike: {
      executor: 'constant-arrival-rate',
      rate: 800,
      timeUnit: '1s',
      duration: '15s',
      preAllocatedVUs: 400,
      maxVUs: 1200,
      startTime: '30s',
      exec: 'mixedTraffic',
      tags: { phase: 'spike' },
    },
    // Phase 3 — cool-down ramp-down to nominal
    cooldown: {
      executor: 'constant-arrival-rate',
      rate: 5,
      timeUnit: '1s',
      duration: '45s',
      preAllocatedVUs: 10,
      maxVUs: 30,
      startTime: '45s',
      exec: 'mixedTraffic',
      tags: { phase: 'cooldown' },
    },
  },
  thresholds: {
    checkout_ok: ['rate>0.90'],
    http_req_failed: ['rate<0.50'],
  },
};

export function mixedTraffic() {
  const headers = {
    'User-Agent': 'k6/predictive-middleware-stress',
    'X-K6-TestRun': __ENV.K6_TEST_RUN || 'part4-validation',
    'X-Correlation-Id': `k6-${__VU}-${__ITER}`,
  };

  const checkoutRes = http.get(CHECKOUT_URL, { headers, tags: { route: 'critical' } });
  const promoRes = http.get(PROMOTIONS_URL, { headers, tags: { route: 'non-critical' } });

  checkoutDuration.add(checkoutRes.timings.duration);
  promotionsDuration.add(promoRes.timings.duration);

  if (checkoutRes.status === 429) throttled429.add(1);
  if (promoRes.status === 429) throttled429.add(1);

  checkoutOk.add(checkoutRes.status === 200);
  promotionsOk.add(promoRes.status === 200);

  check(checkoutRes, { 'checkout available': (r) => r.status === 200 || r.status === 429 });
  check(promoRes, { 'promotions responded': (r) => r.status >= 200 && r.status < 500 });

  sleep(0.01);
}

export function handleSummary(data) {
  const lines = [
    '\n=== Predictive Middleware K6 Summary ===',
    `checkout_ok rate: ${data.metrics.checkout_ok?.values?.rate ?? 'n/a'}`,
    `promotions_ok rate: ${data.metrics.promotions_ok?.values?.rate ?? 'n/a'}`,
    `throttled_429 count: ${data.metrics.throttled_429?.values?.count ?? 0}`,
    `http_req_duration p(99): ${data.metrics.http_req_duration?.values?.['p(99)'] ?? 'n/a'} ms`,
    '========================================\n',
  ];
  return {
    stdout: lines.join('\n'),
  };
}
