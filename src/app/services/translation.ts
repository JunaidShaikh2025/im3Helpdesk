import { Injectable } from '@angular/core';
import { TRANSLATIONS } from '../shared/translations';

@Injectable({ providedIn: 'root' })
export class TranslationService {
  private lang = 'en';

  constructor() {
    this.lang = localStorage.getItem('im3_lang') || 'en';
  }

  setLanguage(lang: string) {
    this.lang = lang;
    localStorage.setItem('im3_lang', lang);
  }

  t(key: string): string {
    return TRANSLATIONS[this.lang]?.[key]
      || TRANSLATIONS['en']?.[key]
      || key;
  }

  getCurrentLang(): string {
    return this.lang;
  }
}