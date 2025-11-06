import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { last, lastValueFrom } from 'rxjs';
import { Nav } from "../layout/nav/nav";
import { AccountService } from '../core/services/account-service';
import { User } from '../types/user';
import { Router, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [Nav, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private accountService = inject(AccountService);
  protected router = inject(Router);
  private http = inject(HttpClient);
  protected readonly title = signal('Dating App');
  protected members = signal<User[]>([]);

  async ngOnInit() {
    this.members.set(await this.getMembers());
    this.setCurrentUser();
  }

  setCurrentUser() {
    const userJson = localStorage.getItem('user');  
    if (userJson) {
      const user = JSON.parse(userJson);
      this.accountService.currentUser.set(user);
    }
  }

  async getMembers() {
    try {
      return lastValueFrom(this.http.get<User[]>('http://localhost:5001/api/members'));
    } catch (error) {
      console.log(error);
      throw error;
    }
  }
}
