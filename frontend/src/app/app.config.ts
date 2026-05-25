import { ApplicationConfig } from '@angular/core';
import { DATE_PIPE_DEFAULT_OPTIONS } from '@angular/common';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideToastr } from 'ngx-toastr';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
	providers: [
		provideRouter(routes),
		provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
		{ provide: DATE_PIPE_DEFAULT_OPTIONS, useValue: { timezone: 'Asia/Kolkata' } },
		provideAnimations(),
		provideToastr({
			timeOut: 2500,
			positionClass: 'toast-top-right',
			preventDuplicates: true,
			progressBar: true,
			closeButton: false,
			tapToDismiss: true,
			newestOnTop: true,
			maxOpened: 3
		})
	]
};