import { HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast-service';
import { inject } from '@angular/core/primitives/di';
import { Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastService = inject(ToastService);
  const router = inject(Router);
  return next(req).pipe(
    catchError(err => {
      if (err) {
        switch (err.status) {
          case 400:
            toastService.error('Bad Request: ' + err.error);
            break;
          case 401:
            toastService.error('Unauthorized');
            break;
          case 404:
            toastService.error('Not Found');
            break;
          case 500:
            toastService.error('Server Error');
            break;
          default:
            toastService.error('Something went wrong');
        }
      }
      throw err;
    })
  )
};
