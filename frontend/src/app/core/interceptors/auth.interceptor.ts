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
  AUTH_EXCLUDED_URLS,
  TOKEN_KEY
} from '../constants/auth.constants';

const RETRY_HEADER = 'x-auth-retry';
let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

function isApiRequest(url: string): boolean {
  return url.startsWith(environment.apiUrl);
}

function isExcludedAuthRequest(url: string): boolean {
  const normalizedUrl = url.toLowerCase();
  return AUTH_EXCLUDED_URLS.some(excluded =>
    normalizedUrl.includes(excluded)
  );
}

function buildRequestWithToken(
  req: HttpRequest<unknown>
): HttpRequest<unknown> {
  if (!isApiRequest(req.url) || isExcludedAuthRequest(req.url)) {
    return req;
  }

  const token = localStorage.getItem(TOKEN_KEY);
  const headers = token
    ? { Authorization: `Bearer ${token}` }
    : undefined;

  return req.clone({
    setHeaders: headers,
    withCredentials: true
  });
}

function withRetryMarker(req: HttpRequest<unknown>): HttpRequest<unknown> {
  return req.clone({
    setHeaders: {
      [RETRY_HEADER]: '1'
    }
  });
}

function retryWithLatestToken(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> {
  const retryReq = withRetryMarker(buildRequestWithToken(req));
  return next(retryReq);
}

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const authService = inject(AuthService);
  const authReq = buildRequestWithToken(req);

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      const isUnauthorized = error.status === 401;
      const isForbidden = error.status === 403;
      const excluded = isExcludedAuthRequest(req.url);
      const alreadyRetried = req.headers.has(RETRY_HEADER);

      if (!isUnauthorized || isForbidden || excluded || alreadyRetried) {
        return throwError(() => error);
      }

      if (isRefreshing) {
        return refreshTokenSubject.pipe(
          filter((token): token is string => !!token),
          take(1),
          switchMap(() => retryWithLatestToken(req, next))
        );
      }

      isRefreshing = true;
      refreshTokenSubject.next(null);

      return authService.refreshToken().pipe(
        switchMap(() => {
          const token = authService.getToken();
          if (!token) {
            authService.logout();
            return throwError(() => error);
          }

          refreshTokenSubject.next(token);
          return retryWithLatestToken(req, next);
        }),
        catchError(refreshError => {
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