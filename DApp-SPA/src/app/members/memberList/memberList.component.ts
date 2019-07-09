import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/user';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { Pagination, PaginatedResult } from 'src/app/_models/pagination';
import { PaginationComponent } from 'ngx-Bootstrap';

@Component({
  selector: 'app-memberList',
  templateUrl: './memberList.component.html',
  styleUrls: ['./memberList.component.css']
})
export class MemberListComponent implements OnInit {

  users: User[];
  pagination: Pagination;
  user: User;
  genderList = [{value: 'female', display: 'Females'}, {value: 'male', display: 'Males'}];
  userParams: any = {};

  constructor(private userService: UserService, private alertifyService: AlertifyService, private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.users = data['users'].result;
      this.pagination = data['users'].pagination;
    });

    this.user = JSON.parse(localStorage.getItem('user'));
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.maxAge = 1500;
    this.userParams.minAge = 18;
    this.userParams.orderBy = 'lastActive';
    // this.loadUsers();
  }

  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    this.loadUsers();
  }

  resetFilters() {
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.maxAge = 1500;
    this.userParams.minAge = 18;
    this.loadUsers();
  }

  loadUsers(): void {
    this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, this.userParams)
      .subscribe((res: PaginatedResult<User[]>) => {
        this.users = res.result;
        this.pagination = res.pagination;
      }, error => {
        this.alertifyService.error(error);
      });
  }
}
