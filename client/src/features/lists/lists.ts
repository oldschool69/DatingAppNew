import { Component, inject, OnInit, signal } from '@angular/core';
import { LikesService } from '../../core/services/likes-service';
import { Member } from '../../types/member';
import { MemberCard } from "../members/member-card/member-card";
import { PaginatedResult } from '../../types/pagination';
import { Paginator } from "../../shared/paginator/paginator";

@Component({
  selector: 'app-lists',
  imports: [MemberCard, Paginator],
  templateUrl: './lists.html',
  styleUrl: './lists.css'
})
export class Lists implements OnInit {
  private likesService = inject(LikesService);
  protected members = signal<Member[]>([]);
  protected predicate = 'liked';
  protected paginatedMembers = signal<PaginatedResult<Member> | null>(null);
  // protected memberParams = new MemberParams();
  // protected updatedParams = new MemberParams();
  
  
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
        this.paginatedMembers.set({ items: members, metadata: { currentPage: 1, totalPages: 1, pageSize: members.length, totalCount: members.length } });
      }
    });
  }

  onPageChange(event: { pageNumber: number; pageSize: number }){
    // this.memberParams.pageNumber = event.pageNumber;
    // this.memberParams.pageSize = event.pageSize;
    this.loadLikes();
  }
}
