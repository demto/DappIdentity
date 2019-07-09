import { Resolve, ActivatedRouteSnapshot, Route, Router } from '@angular/router';
import { User } from '../_models/user';
import { Observable, of } from 'rxjs';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { UserService } from '../_services/user.service';
import { catchError } from 'rxjs/operators';
import { Injectable } from '@angular/core';

@Injectable()

export class ListsResolver implements Resolve<User[]> {
    pageNumber = 1;
    pageSize = 5;
    likesParam = 'Likers';

    constructor(private router: Router,
        private alertify: AlertifyService,
        private userService: UserService) { }

    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
        return this.userService.getUsers(this.pageNumber, this.pageSize, null, this.likesParam)
            .pipe(catchError(error => {
                this.alertify.error('Problem retrieving data');
                this.router.navigate(['/home']);
                return of(null);
            }));
    }
}
