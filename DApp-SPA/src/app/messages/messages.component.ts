import { Component, OnInit } from '@angular/core';
import { Pagination, PaginatedResult } from '../_models/pagination';
import { UserService } from '../_services/user.service';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { Message } from '../_models/message';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {

  messages: Message[];
  pagination: Pagination;
  messageContainer = 'Unread;';

  constructor(private userService: UserService, private authService: AuthService,
      private alertify: AlertifyService, private activatedRoute: ActivatedRoute) { }

  ngOnInit() {
    this.activatedRoute.data.subscribe(data => {
      this.messages = data['messages'].result;
      this.pagination = data['messages'].pagination;
    });
  }

  loadMessages() {
    this.userService.getMessages(this.authService.decodedToken.nameid,
        this.pagination.currentPage, this.pagination.itemsPerPage, this.messageContainer)
          .subscribe((res: PaginatedResult<Message[]>) => {
            this.messages = res.result;
            this.pagination = res.pagination;
          }, error => {
            this.alertify.error(error);
          });
  }

  deleteMessage(id: number) {
    this.alertify.confirmMessage('Are you sure you want to delete this message?', () => {
      this.userService.deleteMessage(id, this.authService.decodedToken.nameid).subscribe(() => {
        this.messages.splice(this.messages.findIndex(m => m.messageId === id), 1);
        this.alertify.success('Message has been deleted');
      }, error => {
        this.alertify.error('Message could not have been deleted');
      });
    });
  }

  pageChanged(event: any) {
    this.pagination.currentPage = event.page;
    this.loadMessages();
  }

}
