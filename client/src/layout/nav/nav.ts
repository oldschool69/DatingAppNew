import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../core/services/account-service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-nav',
  imports: [FormsModule],
  templateUrl: './nav.html',
  styleUrl: './nav.css'
})
export class Nav {

  protected accountService = inject(AccountService);
  protected creds: any = {};

  async login(){
    try {
      const result = await firstValueFrom(this.accountService.login(this.creds));
      console.log(result);
      this.creds = {};
    } catch (error: any) {
      alert(error.message);
    }
  }

  logout(){
    this.accountService.logout();
  }

}
