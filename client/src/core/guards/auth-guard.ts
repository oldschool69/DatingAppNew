import { CanActivateFn } from '@angular/router';
import { AccountService } from '../services/account-service';
import { inject } from '@angular/core';
import { ToastService } from '../services/toast-service';

export const authGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toastService = inject(ToastService);

  if (accountService.currentUser()) {
    return true;
  } else {
    toastService.error('You shall not pass! Please log in to access this area.');
    return false;
  }

};
