import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Injectable({
  providedIn: 'root'
})

export class AuthGuard implements CanActivate {

  constructor (private auth: AuthService, private alerify: AlertifyService, private route: Router){
  }

  canActivate(next: ActivatedRouteSnapshot): boolean {
    const roles = next.firstChild.data['roles'] as Array<string>;

    if (roles) {
      const match = this.auth.roleMatch(roles);

      if (match) {
        return true;
      } else {
        this.route.navigate(['members']);
        this.alerify.error('You are not authorised to access this area.');
      }
    }

    if (this.auth.loggedIn()) {
      return true;
    }
    this.alerify.error('You are not logged in!');
    this.route.navigate(['/home']);
    return false;
  }
}
