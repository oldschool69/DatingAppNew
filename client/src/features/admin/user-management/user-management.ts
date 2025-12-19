import { Component, ElementRef, inject, OnInit, signal, ViewChild } from '@angular/core';
import { AdminService } from '../../../core/services/admin-service';
import { User } from '../../../types/user';

@Component({
  selector: 'app-user-management',
  imports: [],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css'
})
export class UserManagement implements OnInit {
  @ViewChild('rolesModal') rolesModal!: ElementRef<HTMLDialogElement>;
  private adminService = inject(AdminService);
  protected users = signal<User[]>([]);
  protected availableRoles = ['Admin', 'Moderator', 'Member'];
  protected selectedUser: User | null = null;

  openRolesModal(user: User) {
    this.selectedUser = user;
    this.rolesModal.nativeElement.showModal();
  }

  closeRolesModal() {
    this.rolesModal.nativeElement.close();
    this.selectedUser = null;
  }

  updateRoleSelection(role: string, event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    if (this.selectedUser) {
      if (checked && !this.selectedUser.roles.includes(role)) {
        this.selectedUser.roles.push(role);
      } else if (!checked && this.selectedUser.roles.includes(role)) {
        this.selectedUser.roles = this.selectedUser.roles.filter(r => r !== role);
      }
    }
  }

  updateUserRoles() {
    if (this.selectedUser) {
      const roles = this.selectedUser.roles;
      this.adminService.updateUserRoles(this.selectedUser.id, roles).subscribe({
        next: updatedRoles => {
          this.users.update(users => users.map(u => {
            if (u.id === this.selectedUser!.id) {
              return { ...u, roles: updatedRoles };
            }
            return u;
          }));
          this.closeRolesModal();
        },
        error: err => console.log('Failed to update roles', err)
      });
    }
  }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe({
      next: users => {
        this.users.set(users);
      }
    });
  }

  toggleRole(event: Event, role: string) {
    if (!this.selectedUser) return;
    const isChecked = (event.target as HTMLInputElement).checked;
    if (isChecked) {
      if (!this.selectedUser.roles.includes(role)) {
        this.selectedUser.roles.push(role);
      }
    } else {
      this.selectedUser.roles = this.selectedUser.roles.filter(r => r !== role);
    }
  }

}
