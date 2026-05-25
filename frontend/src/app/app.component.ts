import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './core/services/theme.service';
import { IconStyleService } from './core/services/icon-style.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  private themeService = inject(ThemeService);
  private iconStyleService = inject(IconStyleService);

  ngOnInit(): void {
    this.themeService.initTheme();
    this.iconStyleService.init();
  }
}
