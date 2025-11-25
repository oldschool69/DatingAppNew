import { Component, input, output, signal } from '@angular/core';

@Component({
  selector: 'app-favorite-button',
  imports: [],
  templateUrl: './favorite-button.html',
  styleUrl: './favorite-button.css'
})
export class FavoriteButton {
  isProfilePhoto = input<boolean>(false);
  disabled = input<boolean>(false);
  selected = input<boolean>(false);
  clickEvent = output<Event>();

  onClick(event: Event) {
    this.clickEvent.emit(event);
  }
}
