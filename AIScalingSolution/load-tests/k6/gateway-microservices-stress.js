/**
 * K6 stress blueprint — ApiGateway + microservices (predictive middleware at ingress)
 *
 * Route classes (configured in ApiGateway appsettings):
 *   Critical:     GET /orders, GET /users
 *   Non-critical: GET /products  (shed with HTTP 429 when posture is Critical)
 *
 * Run (full stack via docker compose):
 *   docker compose up -d
 *   k6 run -e BASE_URL=http://localhost:5000 load-tests/k6/gateway-microservices-stress.js
 */
import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const ORDERS_URL = `${BASE_URL}/orders`;
const PRODUCTS_URL = `${BASE_URL}/products`;

const ordersOk = new Rate('orders_ok');
const productsOk = new Rate('products_ok');
const throttled429 = new Counter('throttled_429');
const ordersDuration = new Trend('orders_duration', true);
const productsDuration = new Trend('products_duration', true);

export const options = {
  scenarios: {
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
    orders_ok: ['rate>0.90'],
    http_req_failed: ['rate<0.50'],
  },
};

export function mixedTraffic() {
  const headers = {
    'User-Agent': 'k6/gateway-microservices-stress',
    'X-K6-TestRun': __ENV.K6_TEST_RUN || 'gateway-validation',
    'X-Correlation-Id': `k6-${__VU}-${__ITER}`,
  };

  const ordersRes = http.get(ORDERS_URL, { headers, tags: { route: 'critical' } });
  const productsRes = http.get(PRODUCTS_URL, { headers, tags: { route: 'non-critical' } });

  ordersDuration.add(ordersRes.timings.duration);
  productsDuration.add(productsRes.timings.duration);

  if (ordersRes.status === 429) throttled429.add(1);
  if (productsRes.status === 429) throttled429.add(1);

  ordersOk.add(ordersRes.status === 200);
  productsOk.add(productsRes.status === 200);

  check(ordersRes, { 'orders available': (r) => r.status === 200 || r.status === 429 });
  check(productsRes, { 'products responded': (r) => r.status >= 200 && r.status < 500 });

  sleep(0.01);
}

export function handleSummary(data) {
  const lines = [
    '\n=== ApiGateway Microservices K6 Summary ===',
    `orders_ok rate: ${data.metrics.orders_ok?.values?.rate ?? 'n/a'}`,
    `products_ok rate: ${data.metrics.products_ok?.values?.rate ?? 'n/a'}`,
    `throttled_429 count: ${data.metrics.throttled_429?.values?.count ?? 0}`,
    `http_req_duration p(99): ${data.metrics.http_req_duration?.values?.['p(99)'] ?? 'n/a'} ms`,
    '============================================\n',
  ];
  return {
    stdout: lines.join('\n'),
  };
}
