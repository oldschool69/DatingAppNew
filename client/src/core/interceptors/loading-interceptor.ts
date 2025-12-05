import { HttpEvent, HttpInterceptorFn, HttpParams } from '@angular/common/http';
import { BusyService } from '../services/busy-service';
import { inject } from '@angular/core';
import { delay, finalize, of, tap } from 'rxjs';

const cache = new Map<string, HttpEvent<unknown>>();

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);

  const generateCashKey = (url: string, params: HttpParams): string => {
    const paramsString = params.keys().map(key => `${key}=${params.get(key)}`).join('&');
    return paramsString ? `${url}?${paramsString}` : url;
  }

  const cacheKey = generateCashKey(req.url, req.params);

  if (req.method === 'GET') {
    const cachedResponse = cache.get(cacheKey);
    if (cachedResponse) {
      return of(cachedResponse);
    }
  }

  busyService.busy();
  return next(req).pipe(
    delay(500), // Simulate network delay for demonstration purposes
    tap(response => {
        cache.set(cacheKey, response);
    }),
    finalize(() => {
      busyService.idle();
    })
  );
};
