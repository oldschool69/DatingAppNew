import { Component, inject, OnInit, signal } from '@angular/core';
import { MemberService } from '../../../core/services/member-service';
import { ActivatedRoute } from '@angular/router';
import { Member, Photo } from '../../../types/member';
import { ImageUpload } from "../../../shared/image-upload/image-upload";
import { AccountService } from '../../../core/services/account-service';
import { User } from '../../../types/user';
import { FavoriteButton } from "../../../shared/favorite-button/favorite-button";
import { DeleteButton } from "../../../shared/delete-button/delete-button";

@Component({
  selector: 'app-member-photos',
  imports: [ImageUpload, FavoriteButton, DeleteButton],
  templateUrl: './member-photos.html',
  styleUrl: './member-photos.css'
})
export class MemberPhotos implements OnInit {
  protected memberService = inject(MemberService);
  private route = inject(ActivatedRoute);
  protected accountService = inject(AccountService);
  protected photos = signal<Photo[]>([]);
  protected loading = signal(false);

  ngOnInit(): void {
    const memberId = this.route.parent?.snapshot.paramMap.get('id');
    if(memberId) {
      this.memberService.getMemberPhotos(memberId).subscribe(photos => {
        this.photos.set(photos);
      });
    }
  }

  onUploadImage(file: File) {
    this.loading.set(true);
    this.memberService.uploadPhoto(file).subscribe({
      next: (photo) => {
        this.memberService.editMode.set(false);
        this.loading.set(false);
        this.photos.update(photos => [...photos, photo]);
      },
      error: (err) => {
        console.log('Error uploading photo ', err);
        this.loading.set(false);
      }
    });
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo).subscribe({
      next: () => {
        const currentUser = this.accountService.currentUser();
        if (currentUser) currentUser.imageUrl = photo.url;
        this.accountService.setCurrentUser(currentUser as User);
        this.memberService.member.update(member => ({
          ...member,
          imageUrl: photo.url
        }) as Member)
      },
      error: (err) => {
        console.log('Error setting main photo ', err);
      }
    });
  }

  deletePhoto(photoId: number) {
    this.memberService.deletePhoto(photoId).subscribe({
      next: () => {
        this.photos.update(photos => photos.filter(p => p.id !== photoId));
      },
      error: (err) => {
        console.log('Error deleting photo ', err);
      }
    });
  }
}
