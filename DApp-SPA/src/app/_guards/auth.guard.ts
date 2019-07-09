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

  canActivate(): boolean {
    if (this.auth.loggedIn()) {
      return true;
    }
    this.alerify.error('You are not logged in!');
    this.route.navigate(['/home']);
    return false;
  }
}
