import { Component, inject, OnInit, signal } from '@angular/core';
import { LikesService } from '../../core/services/likes-service';
import { LikedParams, Member } from '../../types/member';
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
  protected likedParams = new LikedParams();
  protected updatedLikedParams = new LikedParams();


  tabs = [
    { label: 'Liked', value: 'liked' },
    { label: 'Liked me', value: 'likedBy' },
    { label: 'Mutual', value: 'mutual' },
  ];

  constructor() {
    this.likedParams.predicate = this.predicate;
  }

  ngOnInit(): void {
    this.loadLikes();
  }

  setPredicate(predicate: string): void {
    if (this.predicate === predicate) return;
    this.predicate = predicate;
    this.likedParams.predicate = predicate;
    this.loadLikes();
  }

  loadLikes(): void {
    this.likesService.getLikes(this.likedParams).subscribe(result => {
      this.paginatedMembers.set(result);
    });
  }

  onPageChange(event: { pageNumber: number; pageSize: number }){
    this.likedParams.pageNumber = event.pageNumber;
    this.likedParams.pageSize = event.pageSize;
    this.loadLikes();
  }
}
