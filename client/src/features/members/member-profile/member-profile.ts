import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../../../types/member';
import { DatePipe } from '@angular/common';
import { MemberService } from '../../../core/services/member-service';

@Component({
  selector: 'app-member-profile',
  imports: [DatePipe],
  templateUrl: './member-profile.html',
  styleUrl: './member-profile.css'
})
export class MemberProfile implements OnInit {
  private route = inject(ActivatedRoute);
  protected member = signal<Member | undefined>(undefined);
  protected memberService = inject(MemberService);

  ngOnInit(): void {
    this.route.parent?.data.subscribe({
      next: data => {
        this.member.set(data['member']);
      }
    });
  }

}
