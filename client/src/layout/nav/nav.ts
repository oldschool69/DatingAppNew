import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';
import { firstValueFrom } from 'rxjs';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastService } from '../../core/services/toast-service';

@Component({
  selector: 'app-nav',
  imports: [FormsModule, RouterLink, RouterLinkActive],
  templateUrl: './nav.html',
  styleUrl: './nav.css'
})
export class Nav {

  protected accountService = inject(AccountService);
  protected creds: any = {};
  private router = inject(Router);
  private toast = inject(ToastService);

  async login(){
    try {
      await firstValueFrom(this.accountService.login(this.creds));
      this.router.navigateByUrl('/members');
      this.toast.sucess('Login successful');
      this.creds = {};
    } catch (error: any) {
      console.log('***DEBUG error ',error);
      this.toast.error('Login failed: ' + error.error);
    }
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

}
