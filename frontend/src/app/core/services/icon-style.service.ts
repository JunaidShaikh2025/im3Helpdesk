import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type IconStyleId =
  | 'outline'
  | 'colorful'
  | 'awesome'
  | 'emoji'
  | 'soft'
  | 'soft-colorful'
  | 'awesome-colorful'
  | 'mono';

@Injectable({ providedIn: 'root' })
export class IconStyleService {
  private readonly storageKey = 'im3_icon_style';
  private readonly fallback: IconStyleId = 'outline';

  private readonly iconStyleSubject = new BehaviorSubject<IconStyleId>(this.fallback);
  readonly iconStyle$ = this.iconStyleSubject.asObservable();

  current(): IconStyleId {
    return this.iconStyleSubject.value;
  }

  init(): IconStyleId {
    const saved = localStorage.getItem(this.storageKey);
    return this.apply(saved);
  }

  apply(iconStyleId: string | null | undefined): IconStyleId {
    const next = this.resolve(iconStyleId);

    localStorage.setItem(this.storageKey, next);
    this.iconStyleSubject.next(next);

    // Used by global CSS selectors (layout swaps SVG/emoji + stroke width).
    document.body.dataset['im3IconStyle'] = next;

    return next;
  }

  private resolve(iconStyleId: string | null | undefined): IconStyleId {
    // Backwards compatibility (older builds):
    // - thin/bold were just stroke-width variants; treat as outline.
    if (iconStyleId === 'thin' || iconStyleId === 'bold') return 'outline';

    // Backwards compatibility: earlier colorful/awesome/emoji remain.

    if (
      iconStyleId === 'outline' ||
      iconStyleId === 'colorful' ||
      iconStyleId === 'awesome' ||
      iconStyleId === 'emoji' ||
      iconStyleId === 'soft' ||
      iconStyleId === 'soft-colorful' ||
      iconStyleId === 'awesome-colorful' ||
      iconStyleId === 'mono'
    ) {
      return iconStyleId;
    }
    return this.fallback;
  }
}
