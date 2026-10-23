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
import { AuthService } from '../services/auth.service';
import {
  AUTH_EXCLUDED_URLS,
  TOKEN_KEY
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
  req: HttpRequest<unknown>,
  token?: string | null
): HttpRequest<unknown> {
  if (!isApiRequest(req.url)) {
    return req;
  }

  const setHeaders: Record<string, string> = {};
  if (token) {
    setHeaders['Authorization'] = `Bearer ${token}`;
  }

  return req.clone({
    withCredentials: true,
    setHeaders
  });
}

function withRetryMarker(req: HttpRequest<unknown>, token?: string | null): HttpRequest<unknown> {
  const setHeaders: Record<string, string> = {
    [RETRY_HEADER]: '1'
  };
  if (token) {
    setHeaders['Authorization'] = `Bearer ${token}`;
  }

  return req.clone({
    withCredentials: true,
    setHeaders
  });
}

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> => {
  const authService = inject(AuthService);
  const token = localStorage.getItem(TOKEN_KEY) || authService.getToken();
  const authReq = buildRequestWithCredentials(req, token);

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
            const currentToken = localStorage.getItem(TOKEN_KEY) || authService.getToken();
            return next(withRetryMarker(req, currentToken));
          })
        );
      }

      // Acquire refresh lock
      isRefreshing = true;
      refreshStatus$.next(null);

      return authService.refreshToken().pipe(
        switchMap(() => {
          refreshStatus$.next(true);
          const currentToken = localStorage.getItem(TOKEN_KEY) || authService.getToken();
          return next(withRetryMarker(req, currentToken));
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