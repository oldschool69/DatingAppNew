import { Component, inject, OnInit, signal } from '@angular/core';
import { LikesService } from '../../core/services/likes-service';
import { Member } from '../../types/member';
import { MemberCard } from "../members/member-card/member-card";

@Component({
  selector: 'app-lists',
  imports: [MemberCard],
  templateUrl: './lists.html',
  styleUrl: './lists.css'
})
export class Lists implements OnInit {
  private likesService = inject(LikesService);
  protected members = signal<Member[]>([]);
  protected predicate = 'liked';
  
  tabs = [
    { label: 'Liked', value: 'liked' },
    { label: 'Liked me', value: 'likedBy' },
    { label: 'Mutual', value: 'mutual' },
  ];
  
  ngOnInit(): void {
    this.loadLikes();
  }

  setPredicate(predicate: string): void {
    if (this.predicate === predicate) return;
    this.predicate = predicate;
    this.loadLikes();
  }

  loadLikes(): void {
    this.likesService.getLikes(this.predicate).subscribe({
      next: members => {
        this.members.set(members);
      }
    });
  }
}
