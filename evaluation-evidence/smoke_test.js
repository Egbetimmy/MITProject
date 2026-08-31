import http from 'k6/http';
import { check } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const ORDERS_URL = `${BASE_URL}/orders`;
const PRODUCTS_URL = `${BASE_URL}/products`;

export const options = {
  scenarios: {
    smoke: {
      executor: 'constant-arrival-rate',
      rate: 60,
      timeUnit: '1s',
      duration: '60s',
      preAllocatedVUs: 100,
      maxVUs: 1500,
    },
  },
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
};

export default function() {
  const headers = {
    'User-Agent': 'k6/smoke-test',
    'X-Correlation-Id': `k6-smoke-${__VU}-${__ITER}`,
  };

  const ordersRes = http.get(ORDERS_URL, { headers, tags: { route: 'critical' } });
  const productsRes = http.get(PRODUCTS_URL, { headers, tags: { route: 'non-critical' } });

  check(ordersRes, { 'orders status check': (r) => r.status === 200 || r.status === 429 });
  check(productsRes, { 'products status check': (r) => r.status === 200 || r.status === 429 });
}
