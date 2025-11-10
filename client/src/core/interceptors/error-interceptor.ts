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
            if (err.error.errors) {
              const modalStateErrors = [];
              for (const key in err.error.errors) {
                if (err.error.errors[key]) {
                  modalStateErrors.push(err.error.errors[key]);
                }
              }
              throw modalStateErrors.flat();
            } else {
              toastService.error(err.error);
            }
            break;
          case 401:
            toastService.error('Unauthorized');
            break;
          case 404:
            router.navigateByUrl('/not-found');
            break;
          case 500:
            const navigationExtras = { state: { error: err.error } };
            router.navigateByUrl('/server-error', navigationExtras);
            break;
          default:
            toastService.error('Something went wrong');
        }
      }
      throw err;
    })
  )
};
