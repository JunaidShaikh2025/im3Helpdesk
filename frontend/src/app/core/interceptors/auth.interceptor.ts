import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpErrorResponse,
  HttpEvent
} from '@angular/common/http';
import { inject } from '@angular/core';
import {
  BehaviorSubject,
  Observable,
  catchError,
  filter,
  finalize,
  switchMap,
  take,
  throwError
} from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../features/auth/auth.service';
import {
  AUTH_EXCLUDED_URLS
} from '../constants/auth.constants';

const RETRY_HEADER = 'x-auth-retry';
let isRefreshing = false;
const refreshStatus$ = new BehaviorSubject<boolean | null>(null);

function isApiRequest(url: string): boolean {
  return url.startsWith(environment.apiUrl) || url.startsWith(environment.baseUrl);
}

function isExcludedAuthRequest(url: string): boolean {
  const normalizedUrl = url.toLowerCase();
  return AUTH_EXCLUDED_URLS.some(excluded =>
    normalizedUrl.includes(excluded)
  );
}

function buildRequestWithCredentials(
  req: HttpRequest<unknown>
): HttpRequest<unknown> {
  if (!isApiRequest(req.url)) {
    return req;
  }

  return req.clone({
    withCredentials: true
  });
}

function withRetryMarker(req: HttpRequest<unknown>): HttpRequest<unknown> {
  return req.clone({
    withCredentials: true,
    setHeaders: {
      [RETRY_HEADER]: '1'
    }
  });
}

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> => {
  const authService = inject(AuthService);
  const authReq = buildRequestWithCredentials(req);

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      const isUnauthorized = error.status === 401;
      const excluded = isExcludedAuthRequest(req.url);
      const alreadyRetried = req.headers.has(RETRY_HEADER);

      if (!isUnauthorized || excluded || alreadyRetried) {
        return throwError(() => error);
      }

      // If another request already triggered a refresh, queue this request
      if (isRefreshing) {
        return refreshStatus$.pipe(
          filter((success): success is boolean => success !== null),
          take(1),
          switchMap(success => {
            if (!success) {
              return throwError(() => error);
            }
            return next(withRetryMarker(req));
          })
        );
      }

      // Acquire refresh lock
      isRefreshing = true;
      refreshStatus$.next(null);

      return authService.refreshToken().pipe(
        switchMap(() => {
          refreshStatus$.next(true);
          return next(withRetryMarker(req));
        }),
        catchError(refreshError => {
          refreshStatus$.next(false);
          authService.logout();
          return throwError(() => refreshError);
        }),
        finalize(() => {
          isRefreshing = false;
        })
      );
    })
  );
};