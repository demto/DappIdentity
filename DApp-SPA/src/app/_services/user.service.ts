import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { User } from '../_models/user';
import { Observable } from 'rxjs';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';
import { Message } from '../_models/message';

// Below is not needed anymore as JWT token getter was used in app module
// const httpOptions = {
//   headers: new HttpHeaders({
//     'Authorization': 'Bearer ' + localStorage.getItem('token')
//   })
// };

@Injectable({
  providedIn: 'root'
})
export class UserService {

  baseUrl = environment.apiUrl;

constructor(private httpClient: HttpClient) { }

  getUsers(page?, itemsPerPage?, userParams?, likesParam?): Observable<PaginatedResult<User[]>> {
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();
    let params = new HttpParams();

    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    if (userParams != null) {
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('minAge', userParams.minAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
    }

    if (likesParam === 'Likers') {
      params = params.append('likers', 'true');
    }

    if (likesParam === 'Likees') {
      params = params.append('likees', 'true');
    }


    return this.httpClient.get<User[]>(this.baseUrl + 'users', { observe: "response", params })
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') != null) {
            paginatedResult.pagination = JSON.parse(
              response.headers.get('Pagination'));
          }
          // paginatedResult.pagination = {currentPage: 1, totalItems: 9, totalPages: 3, itemsPerPage: 5};
          return paginatedResult;
        })
      ); // , httpOptions);
  }

  getUser(userId: number): Observable<User> {
    return this.httpClient.get<User>(this.baseUrl + 'users/' + userId); // , httpOptions);
  }

  updateUser(userId: number, user: User) {
    return this.httpClient.put(this.baseUrl + 'users/' + userId, user);
  }

  setMainPhoto(userId: number, photoId: number) {
    return this.httpClient.post(this.baseUrl + 'users/' + userId + '/photos/' + photoId + '/setmain', {});
  }

  deletePhoto(userId: number, photoId: number) {
    return this.httpClient.delete(this.baseUrl + 'users/' + userId + '/photos/' + photoId);
  }

  sendLike(id: number, recipientId: number) {
    return this.httpClient.post(`${this.baseUrl}users/${id}/like/${recipientId}`, {});
  }

  getMessages(id: number, page?, itemsPerPage?, messageContainer?: string) {
    const paginatedResult: PaginatedResult<Message[]> = new PaginatedResult<Message[]>();
    let params = new HttpParams();
    params = params.append('MessageContainer', messageContainer);

    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    return this.httpClient.get<Message[]>(this.baseUrl + 'users/' + id + '/messages', {observe: 'response', params})
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') !== null) {
            paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
          }

          return paginatedResult;
        })
      );
  }

  getMessageThreads(id: number, recipientId: number) {
    return this.httpClient.get<Message[]>(`${this.baseUrl}users/${id}/messages/thread/${recipientId}`);
  }

  sendMessage(id: number, message: Message) {
    return this.httpClient.post(`${this.baseUrl}users/${id}/messages`, message);
  }

  deleteMessage(id: number, userId: number) {
    return this.httpClient.post(`${this.baseUrl}users/${userId}/messages/${id}`, {});
  }

  markAsRead(userId: number, messageId: number) {
    this.httpClient.post(`${this.baseUrl}users/${userId}/messages/${messageId}/read`, {})
      .subscribe();
  }
}
