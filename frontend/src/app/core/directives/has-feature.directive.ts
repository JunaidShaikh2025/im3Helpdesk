import { Directive, Input, TemplateRef, ViewContainerRef, effect, inject } from '@angular/core';
import { SubscriptionService } from '../services/subscription';

/**
 * Structural directive that only renders the template if the current
 * organization's active subscription includes the requested feature key.
 *
 *   <button *hasFeature="'whatsapp'">Send WhatsApp</button>
 *
 * Accepts an optional alternate template via `hasFeatureElse`.
 */
@Directive({
  selector: '[hasFeature]',
  standalone: true,
})
export class HasFeatureDirective {
  private readonly tpl = inject(TemplateRef<unknown>);
  private readonly vc = inject(ViewContainerRef);
  private readonly sub = inject(SubscriptionService);

  private currentKey = '';
  private elseTpl: TemplateRef<unknown> | null = null;
  private rendered: 'main' | 'else' | 'none' = 'none';

  constructor() {
    // Re-render whenever the feature set changes
    effect(() => {
      // touch the signal so this effect re-fires on changes
      this.sub.features();
      this.update();
    });
    // Trigger initial load if not yet loaded; subscribe ignored intentionally
    this.sub.ensureLoaded().subscribe({ error: () => void 0 });
  }

  @Input({ required: true }) set hasFeature(key: string) {
    this.currentKey = (key || '').toLowerCase();
    this.update();
  }

  @Input() set hasFeatureElse(tpl: TemplateRef<unknown> | null) {
    this.elseTpl = tpl;
    this.update();
  }

  private update(): void {
    const allowed = this.currentKey && this.sub.hasFeature(this.currentKey);
    if (allowed) {
      if (this.rendered !== 'main') {
        this.vc.clear();
        this.vc.createEmbeddedView(this.tpl);
        this.rendered = 'main';
      }
    } else if (this.elseTpl) {
      if (this.rendered !== 'else') {
        this.vc.clear();
        this.vc.createEmbeddedView(this.elseTpl);
        this.rendered = 'else';
      }
    } else if (this.rendered !== 'none') {
      this.vc.clear();
      this.rendered = 'none';
    }
  }
}
