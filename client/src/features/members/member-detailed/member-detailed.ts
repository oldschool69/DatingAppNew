import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { AccountService } from '../../../core/services/account-service';
import { PresenceService } from '../../../core/services/presence-service';
import { LikesService } from '../../../core/services/likes-service';

@Component({
  selector: 'app-member-detailed',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, AgePipe],
  templateUrl: './member-detailed.html',
  styleUrl: './member-detailed.css'
})
export class MemberDetailed implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private accountService = inject(AccountService);
  private routeId = signal<string | null>(null);
  protected memberService = inject(MemberService);
  protected title = signal<string | undefined>('Profile');
  protected presenceService = inject(PresenceService);
  protected isCurrentUser = computed(() => {
    return this.accountService.currentUser()?.id === this.routeId();
  });
  protected likesService = inject(LikesService);
  protected hasLiked = computed(() => {
    return this.likesService.likeIds().includes(this.routeId()!);
  });

  constructor() {
    this.route.paramMap.subscribe({
      next: params => {
        this.routeId.set(params.get('id'));
      }
    });
  }

  ngOnInit(): void {
    this.title.set(this.route.firstChild?.snapshot.title);

    this.router.events.pipe(filter(event => event instanceof  NavigationEnd))
    .subscribe({
      next: () => {
        this.title.set(this.route.firstChild?.snapshot.title);
      }
    });
  }
}
