import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

// Apply saved theme on startup
const savedTheme = localStorage.getItem('im3_theme') || 'theme-blue';
document.body.classList.add(savedTheme);

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));