import {
  UniqueSelectionDispatcher
} from "./chunk-L3FZVU52.js";
import {
  CdkPortalOutlet,
  PortalModule,
  TemplatePortal
} from "./chunk-HNFXGYRO.js";
import {
  _StructuralStylesLoader
} from "./chunk-V45CQF64.js";
import {
  ENTER,
  FocusKeyManager,
  FocusMonitor,
  SPACE,
  _IdGenerator,
  hasModifierKey
} from "./chunk-57ZF7TAI.js";
import "./chunk-XA6252L2.js";
import {
  _animationsDisabled
} from "./chunk-6YA3EALH.js";
import "./chunk-GKVNUTNV.js";
import "./chunk-ENC6VFIZ.js";
import "./chunk-B4BO3VX5.js";
import {
  _CdkPrivateStyleLoader
} from "./chunk-34LCJAFP.js";
import "./chunk-FDV2MSRL.js";
import "./chunk-N4DOILP3.js";
import "./chunk-XEZLRTI4.js";
import "./chunk-H4HMK75K.js";
import "./chunk-YO2PVG56.js";
import "./chunk-NPEN54ZG.js";
import "./chunk-H4ZZ2WUC.js";
import {
  BidiModule
} from "./chunk-GJERZTUN.js";
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ContentChild,
  ContentChildren,
  DOCUMENT,
  Directive,
  ElementRef,
  EventEmitter,
  HostAttributeToken,
  InjectionToken,
  Input,
  NgModule,
  NgZone,
  Output,
  QueryList,
  Renderer2,
  TemplateRef,
  ViewChild,
  ViewContainerRef,
  ViewEncapsulation,
  booleanAttribute,
  inject,
  numberAttribute,
  setClassMetadata,
  signal,
  ɵɵInheritDefinitionFeature,
  ɵɵNgOnChangesFeature,
  ɵɵProvidersFeature,
  ɵɵadvance,
  ɵɵattribute,
  ɵɵclassProp,
  ɵɵconditional,
  ɵɵconditionalCreate,
  ɵɵcontentQuery,
  ɵɵdefineComponent,
  ɵɵdefineDirective,
  ɵɵdefineInjector,
  ɵɵdefineNgModule,
  ɵɵdomElement,
  ɵɵdomElementEnd,
  ɵɵdomElementStart,
  ɵɵelementEnd,
  ɵɵelementStart,
  ɵɵgetInheritedFactory,
  ɵɵlistener,
  ɵɵloadQuery,
  ɵɵnamespaceSVG,
  ɵɵprojection,
  ɵɵprojectionDef,
  ɵɵproperty,
  ɵɵqueryRefresh,
  ɵɵstyleProp,
  ɵɵtemplate,
  ɵɵviewQuery
} from "./chunk-3BW4HTRP.js";
import {
  merge
} from "./chunk-HWYXSU2G.js";
import "./chunk-JRFR6BLO.js";
import {
  EMPTY,
  Subject,
  Subscription,
  filter,
  startWith,
  take
} from "./chunk-MARUHEWW.js";
import "./chunk-XSXJULYR.js";

// node_modules/@angular/cdk/fesm2022/accordion.mjs
var CDK_ACCORDION = new InjectionToken("CdkAccordion");
var CdkAccordion = class _CdkAccordion {
  _stateChanges = new Subject();
  _openCloseAllActions = new Subject();
  id = inject(_IdGenerator).getId("cdk-accordion-");
  multi = false;
  openAll() {
    if (this.multi) {
      this._openCloseAllActions.next(true);
    }
  }
  closeAll() {
    this._openCloseAllActions.next(false);
  }
  ngOnChanges(changes) {
    this._stateChanges.next(changes);
  }
  ngOnDestroy() {
    this._stateChanges.complete();
    this._openCloseAllActions.complete();
  }
  static ɵfac = function CdkAccordion_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _CdkAccordion)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _CdkAccordion,
    selectors: [["cdk-accordion"], ["", "cdkAccordion", ""]],
    inputs: {
      multi: [2, "multi", "multi", booleanAttribute]
    },
    exportAs: ["cdkAccordion"],
    features: [ɵɵProvidersFeature([{
      provide: CDK_ACCORDION,
      useExisting: _CdkAccordion
    }]), ɵɵNgOnChangesFeature]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(CdkAccordion, [{
    type: Directive,
    args: [{
      selector: "cdk-accordion, [cdkAccordion]",
      exportAs: "cdkAccordion",
      providers: [{
        provide: CDK_ACCORDION,
        useExisting: CdkAccordion
      }]
    }]
  }], null, {
    multi: [{
      type: Input,
      args: [{
        transform: booleanAttribute
      }]
    }]
  });
})();
var CdkAccordionItem = class _CdkAccordionItem {
  accordion = inject(CDK_ACCORDION, {
    optional: true,
    skipSelf: true
  });
  _changeDetectorRef = inject(ChangeDetectorRef);
  _expansionDispatcher = inject(UniqueSelectionDispatcher);
  _openCloseAllSubscription = Subscription.EMPTY;
  closed = new EventEmitter();
  opened = new EventEmitter();
  destroyed = new EventEmitter();
  expandedChange = new EventEmitter();
  id = inject(_IdGenerator).getId("cdk-accordion-child-");
  get expanded() {
    return this._expanded;
  }
  set expanded(expanded) {
    if (this._expanded !== expanded) {
      this._expanded = expanded;
      this.expandedChange.emit(expanded);
      if (expanded) {
        this.opened.emit();
        const accordionId = this.accordion ? this.accordion.id : this.id;
        this._expansionDispatcher.notify(this.id, accordionId);
      } else {
        this.closed.emit();
      }
      this._changeDetectorRef.markForCheck();
    }
  }
  _expanded = false;
  get disabled() {
    return this._disabled();
  }
  set disabled(value) {
    this._disabled.set(value);
  }
  _disabled = signal(false, ...ngDevMode ? [{
    debugName: "_disabled"
  }] : []);
  _removeUniqueSelectionListener = () => {
  };
  constructor() {
  }
  ngOnInit() {
    this._removeUniqueSelectionListener = this._expansionDispatcher.listen((id, accordionId) => {
      if (this.accordion && !this.accordion.multi && this.accordion.id === accordionId && this.id !== id) {
        this.expanded = false;
      }
    });
    if (this.accordion) {
      this._openCloseAllSubscription = this._subscribeToOpenCloseAllActions();
    }
  }
  ngOnDestroy() {
    this.opened.complete();
    this.closed.complete();
    this.destroyed.emit();
    this.destroyed.complete();
    this._removeUniqueSelectionListener();
    this._openCloseAllSubscription.unsubscribe();
  }
  toggle() {
    if (!this.disabled) {
      this.expanded = !this.expanded;
    }
  }
  close() {
    if (!this.disabled) {
      this.expanded = false;
    }
  }
  open() {
    if (!this.disabled) {
      this.expanded = true;
    }
  }
  _subscribeToOpenCloseAllActions() {
    return this.accordion._openCloseAllActions.subscribe((expanded) => {
      if (!this.disabled) {
        this.expanded = expanded;
      }
    });
  }
  static ɵfac = function CdkAccordionItem_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _CdkAccordionItem)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _CdkAccordionItem,
    selectors: [["cdk-accordion-item"], ["", "cdkAccordionItem", ""]],
    inputs: {
      expanded: [2, "expanded", "expanded", booleanAttribute],
      disabled: [2, "disabled", "disabled", booleanAttribute]
    },
    outputs: {
      closed: "closed",
      opened: "opened",
      destroyed: "destroyed",
      expandedChange: "expandedChange"
    },
    exportAs: ["cdkAccordionItem"],
    features: [ɵɵProvidersFeature([{
      provide: CDK_ACCORDION,
      useValue: void 0
    }])]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(CdkAccordionItem, [{
    type: Directive,
    args: [{
      selector: "cdk-accordion-item, [cdkAccordionItem]",
      exportAs: "cdkAccordionItem",
      providers: [{
        provide: CDK_ACCORDION,
        useValue: void 0
      }]
    }]
  }], () => [], {
    closed: [{
      type: Output
    }],
    opened: [{
      type: Output
    }],
    destroyed: [{
      type: Output
    }],
    expandedChange: [{
      type: Output
    }],
    expanded: [{
      type: Input,
      args: [{
        transform: booleanAttribute
      }]
    }],
    disabled: [{
      type: Input,
      args: [{
        transform: booleanAttribute
      }]
    }]
  });
})();
var CdkAccordionModule = class _CdkAccordionModule {
  static ɵfac = function CdkAccordionModule_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _CdkAccordionModule)();
  };
  static ɵmod = ɵɵdefineNgModule({
    type: _CdkAccordionModule,
    imports: [CdkAccordion, CdkAccordionItem],
    exports: [CdkAccordion, CdkAccordionItem]
  });
  static ɵinj = ɵɵdefineInjector({});
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(CdkAccordionModule, [{
    type: NgModule,
    args: [{
      imports: [CdkAccordion, CdkAccordionItem],
      exports: [CdkAccordion, CdkAccordionItem]
    }]
  }], null, null);
})();

// node_modules/@angular/material/fesm2022/expansion.mjs
var _c0 = ["body"];
var _c1 = ["bodyWrapper"];
var _c2 = [[["mat-expansion-panel-header"]], "*", [["mat-action-row"]]];
var _c3 = ["mat-expansion-panel-header", "*", "mat-action-row"];
function MatExpansionPanel_ng_template_7_Template(rf, ctx) {
}
var _c4 = [[["mat-panel-title"]], [["mat-panel-description"]], "*"];
var _c5 = ["mat-panel-title", "mat-panel-description", "*"];
function MatExpansionPanelHeader_Conditional_4_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵdomElementStart(0, "span", 1);
    ɵɵnamespaceSVG();
    ɵɵdomElementStart(1, "svg", 2);
    ɵɵdomElement(2, "path", 3);
    ɵɵdomElementEnd()();
  }
}
var MAT_ACCORDION = new InjectionToken("MAT_ACCORDION");
var MAT_EXPANSION_PANEL = new InjectionToken("MAT_EXPANSION_PANEL");
var MatExpansionPanelContent = class _MatExpansionPanelContent {
  _template = inject(TemplateRef);
  _expansionPanel = inject(MAT_EXPANSION_PANEL, {
    optional: true
  });
  constructor() {
  }
  static ɵfac = function MatExpansionPanelContent_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatExpansionPanelContent)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _MatExpansionPanelContent,
    selectors: [["ng-template", "matExpansionPanelContent", ""]]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatExpansionPanelContent, [{
    type: Directive,
    args: [{
      selector: "ng-template[matExpansionPanelContent]"
    }]
  }], () => [], null);
})();
var MAT_EXPANSION_PANEL_DEFAULT_OPTIONS = new InjectionToken("MAT_EXPANSION_PANEL_DEFAULT_OPTIONS");
var MatExpansionPanel = class _MatExpansionPanel extends CdkAccordionItem {
  _viewContainerRef = inject(ViewContainerRef);
  _animationsDisabled = _animationsDisabled();
  _document = inject(DOCUMENT);
  _ngZone = inject(NgZone);
  _elementRef = inject(ElementRef);
  _renderer = inject(Renderer2);
  _cleanupTransitionEnd;
  get hideToggle() {
    return this._hideToggle || this.accordion && this.accordion.hideToggle;
  }
  set hideToggle(value) {
    this._hideToggle = value;
  }
  _hideToggle = false;
  get togglePosition() {
    return this._togglePosition || this.accordion && this.accordion.togglePosition;
  }
  set togglePosition(value) {
    this._togglePosition = value;
  }
  _togglePosition;
  afterExpand = new EventEmitter();
  afterCollapse = new EventEmitter();
  _inputChanges = new Subject();
  accordion = inject(MAT_ACCORDION, {
    optional: true,
    skipSelf: true
  });
  _lazyContent;
  _body;
  _bodyWrapper;
  _portal;
  _headerId = inject(_IdGenerator).getId("mat-expansion-panel-header-");
  constructor() {
    super();
    const defaultOptions = inject(MAT_EXPANSION_PANEL_DEFAULT_OPTIONS, {
      optional: true
    });
    this._expansionDispatcher = inject(UniqueSelectionDispatcher);
    if (defaultOptions) {
      this.hideToggle = defaultOptions.hideToggle;
    }
  }
  _hasSpacing() {
    if (this.accordion) {
      return this.expanded && this.accordion.displayMode === "default";
    }
    return false;
  }
  _getExpandedState() {
    return this.expanded ? "expanded" : "collapsed";
  }
  toggle() {
    this.expanded = !this.expanded;
  }
  close() {
    this.expanded = false;
  }
  open() {
    this.expanded = true;
  }
  ngAfterContentInit() {
    if (this._lazyContent && this._lazyContent._expansionPanel === this) {
      this.opened.pipe(startWith(null), filter(() => this.expanded && !this._portal), take(1)).subscribe(() => {
        this._portal = new TemplatePortal(this._lazyContent._template, this._viewContainerRef);
      });
    }
    this._setupAnimationEvents();
  }
  ngOnChanges(changes) {
    this._inputChanges.next(changes);
  }
  ngOnDestroy() {
    super.ngOnDestroy();
    this._cleanupTransitionEnd?.();
    this._inputChanges.complete();
  }
  _containsFocus() {
    if (this._body) {
      const focusedElement = this._document.activeElement;
      const bodyElement = this._body.nativeElement;
      return focusedElement === bodyElement || bodyElement.contains(focusedElement);
    }
    return false;
  }
  _transitionEndListener = ({
    target,
    propertyName
  }) => {
    if (target === this._bodyWrapper?.nativeElement && propertyName === "grid-template-rows") {
      this._ngZone.run(() => {
        if (this.expanded) {
          this.afterExpand.emit();
        } else {
          this.afterCollapse.emit();
        }
      });
    }
  };
  _setupAnimationEvents() {
    this._ngZone.runOutsideAngular(() => {
      if (this._animationsDisabled) {
        this.opened.subscribe(() => this._ngZone.run(() => this.afterExpand.emit()));
        this.closed.subscribe(() => this._ngZone.run(() => this.afterCollapse.emit()));
      } else {
        setTimeout(() => {
          const element = this._elementRef.nativeElement;
          this._cleanupTransitionEnd = this._renderer.listen(element, "transitionend", this._transitionEndListener);
          element.classList.add("mat-expansion-panel-animations-enabled");
        }, 200);
      }
    });
  }
  static ɵfac = function MatExpansionPanel_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatExpansionPanel)();
  };
  static ɵcmp = ɵɵdefineComponent({
    type: _MatExpansionPanel,
    selectors: [["mat-expansion-panel"]],
    contentQueries: function MatExpansionPanel_ContentQueries(rf, ctx, dirIndex) {
      if (rf & 1) {
        ɵɵcontentQuery(dirIndex, MatExpansionPanelContent, 5);
      }
      if (rf & 2) {
        let _t;
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._lazyContent = _t.first);
      }
    },
    viewQuery: function MatExpansionPanel_Query(rf, ctx) {
      if (rf & 1) {
        ɵɵviewQuery(_c0, 5)(_c1, 5);
      }
      if (rf & 2) {
        let _t;
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._body = _t.first);
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._bodyWrapper = _t.first);
      }
    },
    hostAttrs: [1, "mat-expansion-panel"],
    hostVars: 4,
    hostBindings: function MatExpansionPanel_HostBindings(rf, ctx) {
      if (rf & 2) {
        ɵɵclassProp("mat-expanded", ctx.expanded)("mat-expansion-panel-spacing", ctx._hasSpacing());
      }
    },
    inputs: {
      hideToggle: [2, "hideToggle", "hideToggle", booleanAttribute],
      togglePosition: "togglePosition"
    },
    outputs: {
      afterExpand: "afterExpand",
      afterCollapse: "afterCollapse"
    },
    exportAs: ["matExpansionPanel"],
    features: [ɵɵProvidersFeature([{
      provide: MAT_ACCORDION,
      useValue: void 0
    }, {
      provide: MAT_EXPANSION_PANEL,
      useExisting: _MatExpansionPanel
    }]), ɵɵInheritDefinitionFeature, ɵɵNgOnChangesFeature],
    ngContentSelectors: _c3,
    decls: 9,
    vars: 4,
    consts: [["bodyWrapper", ""], ["body", ""], [1, "mat-expansion-panel-content-wrapper"], ["role", "region", 1, "mat-expansion-panel-content", 3, "id"], [1, "mat-expansion-panel-body"], [3, "cdkPortalOutlet"]],
    template: function MatExpansionPanel_Template(rf, ctx) {
      if (rf & 1) {
        ɵɵprojectionDef(_c2);
        ɵɵprojection(0);
        ɵɵelementStart(1, "div", 2, 0)(3, "div", 3, 1)(5, "div", 4);
        ɵɵprojection(6, 1);
        ɵɵtemplate(7, MatExpansionPanel_ng_template_7_Template, 0, 0, "ng-template", 5);
        ɵɵelementEnd();
        ɵɵprojection(8, 2);
        ɵɵelementEnd()();
      }
      if (rf & 2) {
        ɵɵadvance();
        ɵɵattribute("inert", ctx.expanded ? null : "");
        ɵɵadvance(2);
        ɵɵproperty("id", ctx.id);
        ɵɵattribute("aria-labelledby", ctx._headerId);
        ɵɵadvance(4);
        ɵɵproperty("cdkPortalOutlet", ctx._portal);
      }
    },
    dependencies: [CdkPortalOutlet],
    styles: [".mat-expansion-panel {\n  box-sizing: content-box;\n  display: block;\n  margin: 0;\n  overflow: hidden;\n}\n.mat-expansion-panel.mat-expansion-panel-animations-enabled {\n  transition: margin 225ms cubic-bezier(0.4, 0, 0.2, 1), box-shadow 280ms cubic-bezier(0.4, 0, 0.2, 1);\n}\n.mat-expansion-panel {\n  position: relative;\n  background: var(--mat-expansion-container-background-color, var(--mat-sys-surface));\n  color: var(--mat-expansion-container-text-color, var(--mat-sys-on-surface));\n  border-radius: var(--mat-expansion-container-shape, 12px);\n}\n.mat-expansion-panel:not([class*=mat-elevation-z]) {\n  box-shadow: var(--mat-expansion-container-elevation-shadow, 0px 3px 1px -2px rgba(0, 0, 0, 0.2), 0px 2px 2px 0px rgba(0, 0, 0, 0.14), 0px 1px 5px 0px rgba(0, 0, 0, 0.12));\n}\n.mat-accordion .mat-expansion-panel:not(.mat-expanded), .mat-accordion .mat-expansion-panel:not(.mat-expansion-panel-spacing) {\n  border-radius: 0;\n}\n.mat-accordion .mat-expansion-panel:first-of-type {\n  border-top-right-radius: var(--mat-expansion-container-shape, 12px);\n  border-top-left-radius: var(--mat-expansion-container-shape, 12px);\n}\n.mat-accordion .mat-expansion-panel:last-of-type {\n  border-bottom-right-radius: var(--mat-expansion-container-shape, 12px);\n  border-bottom-left-radius: var(--mat-expansion-container-shape, 12px);\n}\n@media (forced-colors: active) {\n  .mat-expansion-panel {\n    outline: solid 1px;\n  }\n}\n\n.mat-expansion-panel-content-wrapper {\n  display: grid;\n  grid-template-rows: 0fr;\n  grid-template-columns: 100%;\n}\n.mat-expansion-panel-animations-enabled .mat-expansion-panel-content-wrapper {\n  transition: grid-template-rows 225ms cubic-bezier(0.4, 0, 0.2, 1);\n}\n.mat-expansion-panel.mat-expanded > .mat-expansion-panel-content-wrapper {\n  grid-template-rows: 1fr;\n}\n@supports not (grid-template-rows: 0fr) {\n  .mat-expansion-panel-content-wrapper {\n    height: 0;\n  }\n  .mat-expansion-panel.mat-expanded > .mat-expansion-panel-content-wrapper {\n    height: auto;\n  }\n}\n@media print {\n  .mat-expansion-panel-content-wrapper {\n    height: 0;\n  }\n  .mat-expansion-panel.mat-expanded > .mat-expansion-panel-content-wrapper {\n    height: auto;\n  }\n}\n\n.mat-expansion-panel-content {\n  display: flex;\n  flex-direction: column;\n  overflow: visible;\n  min-height: 0;\n  visibility: hidden;\n}\n.mat-expansion-panel-animations-enabled .mat-expansion-panel-content {\n  transition: visibility 190ms linear;\n}\n.mat-expansion-panel.mat-expanded > .mat-expansion-panel-content-wrapper > .mat-expansion-panel-content {\n  visibility: visible;\n}\n.mat-expansion-panel-content {\n  font-family: var(--mat-expansion-container-text-font, var(--mat-sys-body-large-font));\n  font-size: var(--mat-expansion-container-text-size, var(--mat-sys-body-large-size));\n  font-weight: var(--mat-expansion-container-text-weight, var(--mat-sys-body-large-weight));\n  line-height: var(--mat-expansion-container-text-line-height, var(--mat-sys-body-large-line-height));\n  letter-spacing: var(--mat-expansion-container-text-tracking, var(--mat-sys-body-large-tracking));\n}\n\n.mat-expansion-panel-body {\n  padding: 0 24px 16px;\n}\n\n.mat-expansion-panel-spacing {\n  margin: 16px 0;\n}\n.mat-accordion > .mat-expansion-panel-spacing:first-child, .mat-accordion > *:first-child:not(.mat-expansion-panel) .mat-expansion-panel-spacing {\n  margin-top: 0;\n}\n.mat-accordion > .mat-expansion-panel-spacing:last-child, .mat-accordion > *:last-child:not(.mat-expansion-panel) .mat-expansion-panel-spacing {\n  margin-bottom: 0;\n}\n\n.mat-action-row {\n  border-top-style: solid;\n  border-top-width: 1px;\n  display: flex;\n  flex-direction: row;\n  justify-content: flex-end;\n  padding: 16px 8px 16px 24px;\n  border-top-color: var(--mat-expansion-actions-divider-color, var(--mat-sys-outline));\n}\n.mat-action-row .mat-button-base,\n.mat-action-row .mat-mdc-button-base {\n  margin-left: 8px;\n}\n[dir=rtl] .mat-action-row .mat-button-base,\n[dir=rtl] .mat-action-row .mat-mdc-button-base {\n  margin-left: 0;\n  margin-right: 8px;\n}\n"],
    encapsulation: 2,
    changeDetection: 0
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatExpansionPanel, [{
    type: Component,
    args: [{
      selector: "mat-expansion-panel",
      exportAs: "matExpansionPanel",
      encapsulation: ViewEncapsulation.None,
      changeDetection: ChangeDetectionStrategy.OnPush,
      providers: [{
        provide: MAT_ACCORDION,
        useValue: void 0
      }, {
        provide: MAT_EXPANSION_PANEL,
        useExisting: MatExpansionPanel
      }],
      host: {
        "class": "mat-expansion-panel",
        "[class.mat-expanded]": "expanded",
        "[class.mat-expansion-panel-spacing]": "_hasSpacing()"
      },
      imports: [CdkPortalOutlet],
      template: `<ng-content select="mat-expansion-panel-header"></ng-content>
<div class="mat-expansion-panel-content-wrapper" [attr.inert]="expanded ? null : ''" #bodyWrapper>
  <div class="mat-expansion-panel-content"
       role="region"
       [attr.aria-labelledby]="_headerId"
       [id]="id"
       #body>
    <div class="mat-expansion-panel-body">
      <ng-content></ng-content>
      <ng-template [cdkPortalOutlet]="_portal"></ng-template>
    </div>
    <ng-content select="mat-action-row"></ng-content>
  </div>
</div>
`,
      styles: [".mat-expansion-panel {\n  box-sizing: content-box;\n  display: block;\n  margin: 0;\n  overflow: hidden;\n}\n.mat-expansion-panel.mat-expansion-panel-animations-enabled {\n  transition: margin 225ms cubic-bezier(0.4, 0, 0.2, 1), box-shadow 280ms cubic-bezier(0.4, 0, 0.2, 1);\n}\n.mat-expansion-panel {\n  position: relative;\n  background: var(--mat-expansion-container-background-color, var(--mat-sys-surface));\n  color: var(--mat-expansion-container-text-color, var(--mat-sys-on-surface));\n  border-radius: var(--mat-expansion-container-shape, 12px);\n}\n.mat-expansion-panel:not([class*=mat-elevation-z]) {\n  box-shadow: var(--mat-expansion-container-elevation-shadow, 0px 3px 1px -2px rgba(0, 0, 0, 0.2), 0px 2px 2px 0px rgba(0, 0, 0, 0.14), 0px 1px 5px 0px rgba(0, 0, 0, 0.12));\n}\n.mat-accordion .mat-expansion-panel:not(.mat-expanded), .mat-accordion .mat-expansion-panel:not(.mat-expansion-panel-spacing) {\n  border-radius: 0;\n}\n.mat-accordion .mat-expansion-panel:first-of-type {\n  border-top-right-radius: var(--mat-expansion-container-shape, 12px);\n  border-top-left-radius: var(--mat-expansion-container-shape, 12px);\n}\n.mat-accordion .mat-expansion-panel:last-of-type {\n  border-bottom-right-radius: var(--mat-expansion-container-shape, 12px);\n  border-bottom-left-radius: var(--mat-expansion-container-shape, 12px);\n}\n@media (forced-colors: active) {\n  .mat-expansion-panel {\n    outline: solid 1px;\n  }\n}\n\n.mat-expansion-panel-content-wrapper {\n  display: grid;\n  grid-template-rows: 0fr;\n  grid-template-columns: 100%;\n}\n.mat-expansion-panel-animations-enabled .mat-expansion-panel-content-wrapper {\n  transition: grid-template-rows 225ms cubic-bezier(0.4, 0, 0.2, 1);\n}\n.mat-expansion-panel.mat-expanded > .mat-expansion-panel-content-wrapper {\n  grid-template-rows: 1fr;\n}\n@supports not (grid-template-rows: 0fr) {\n  .mat-expansion-panel-content-wrapper {\n    height: 0;\n  }\n  .mat-expansion-panel.mat-expanded > .mat-expansion-panel-content-wrapper {\n    height: auto;\n  }\n}\n@media print {\n  .mat-expansion-panel-content-wrapper {\n    height: 0;\n  }\n  .mat-expansion-panel.mat-expanded > .mat-expansion-panel-content-wrapper {\n    height: auto;\n  }\n}\n\n.mat-expansion-panel-content {\n  display: flex;\n  flex-direction: column;\n  overflow: visible;\n  min-height: 0;\n  visibility: hidden;\n}\n.mat-expansion-panel-animations-enabled .mat-expansion-panel-content {\n  transition: visibility 190ms linear;\n}\n.mat-expansion-panel.mat-expanded > .mat-expansion-panel-content-wrapper > .mat-expansion-panel-content {\n  visibility: visible;\n}\n.mat-expansion-panel-content {\n  font-family: var(--mat-expansion-container-text-font, var(--mat-sys-body-large-font));\n  font-size: var(--mat-expansion-container-text-size, var(--mat-sys-body-large-size));\n  font-weight: var(--mat-expansion-container-text-weight, var(--mat-sys-body-large-weight));\n  line-height: var(--mat-expansion-container-text-line-height, var(--mat-sys-body-large-line-height));\n  letter-spacing: var(--mat-expansion-container-text-tracking, var(--mat-sys-body-large-tracking));\n}\n\n.mat-expansion-panel-body {\n  padding: 0 24px 16px;\n}\n\n.mat-expansion-panel-spacing {\n  margin: 16px 0;\n}\n.mat-accordion > .mat-expansion-panel-spacing:first-child, .mat-accordion > *:first-child:not(.mat-expansion-panel) .mat-expansion-panel-spacing {\n  margin-top: 0;\n}\n.mat-accordion > .mat-expansion-panel-spacing:last-child, .mat-accordion > *:last-child:not(.mat-expansion-panel) .mat-expansion-panel-spacing {\n  margin-bottom: 0;\n}\n\n.mat-action-row {\n  border-top-style: solid;\n  border-top-width: 1px;\n  display: flex;\n  flex-direction: row;\n  justify-content: flex-end;\n  padding: 16px 8px 16px 24px;\n  border-top-color: var(--mat-expansion-actions-divider-color, var(--mat-sys-outline));\n}\n.mat-action-row .mat-button-base,\n.mat-action-row .mat-mdc-button-base {\n  margin-left: 8px;\n}\n[dir=rtl] .mat-action-row .mat-button-base,\n[dir=rtl] .mat-action-row .mat-mdc-button-base {\n  margin-left: 0;\n  margin-right: 8px;\n}\n"]
    }]
  }], () => [], {
    hideToggle: [{
      type: Input,
      args: [{
        transform: booleanAttribute
      }]
    }],
    togglePosition: [{
      type: Input
    }],
    afterExpand: [{
      type: Output
    }],
    afterCollapse: [{
      type: Output
    }],
    _lazyContent: [{
      type: ContentChild,
      args: [MatExpansionPanelContent]
    }],
    _body: [{
      type: ViewChild,
      args: ["body"]
    }],
    _bodyWrapper: [{
      type: ViewChild,
      args: ["bodyWrapper"]
    }]
  });
})();
var MatExpansionPanelActionRow = class _MatExpansionPanelActionRow {
  static ɵfac = function MatExpansionPanelActionRow_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatExpansionPanelActionRow)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _MatExpansionPanelActionRow,
    selectors: [["mat-action-row"]],
    hostAttrs: [1, "mat-action-row"]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatExpansionPanelActionRow, [{
    type: Directive,
    args: [{
      selector: "mat-action-row",
      host: {
        class: "mat-action-row"
      }
    }]
  }], null, null);
})();
var MatExpansionPanelHeader = class _MatExpansionPanelHeader {
  panel = inject(MatExpansionPanel, {
    host: true
  });
  _element = inject(ElementRef);
  _focusMonitor = inject(FocusMonitor);
  _changeDetectorRef = inject(ChangeDetectorRef);
  _parentChangeSubscription = Subscription.EMPTY;
  constructor() {
    inject(_CdkPrivateStyleLoader).load(_StructuralStylesLoader);
    const panel = this.panel;
    const defaultOptions = inject(MAT_EXPANSION_PANEL_DEFAULT_OPTIONS, {
      optional: true
    });
    const tabIndex = inject(new HostAttributeToken("tabindex"), {
      optional: true
    });
    const accordionHideToggleChange = panel.accordion ? panel.accordion._stateChanges.pipe(filter((changes) => !!(changes["hideToggle"] || changes["togglePosition"]))) : EMPTY;
    this.tabIndex = parseInt(tabIndex || "") || 0;
    this._parentChangeSubscription = merge(panel.opened, panel.closed, accordionHideToggleChange, panel._inputChanges.pipe(filter((changes) => {
      return !!(changes["hideToggle"] || changes["disabled"] || changes["togglePosition"]);
    }))).subscribe(() => this._changeDetectorRef.markForCheck());
    panel.closed.pipe(filter(() => panel._containsFocus())).subscribe(() => this._focusMonitor.focusVia(this._element, "program"));
    if (defaultOptions) {
      this.expandedHeight = defaultOptions.expandedHeight;
      this.collapsedHeight = defaultOptions.collapsedHeight;
    }
  }
  expandedHeight;
  collapsedHeight;
  tabIndex = 0;
  get disabled() {
    return this.panel.disabled;
  }
  _toggle() {
    if (!this.disabled) {
      this.panel.toggle();
    }
  }
  _isExpanded() {
    return this.panel.expanded;
  }
  _getExpandedState() {
    return this.panel._getExpandedState();
  }
  _getPanelId() {
    return this.panel.id;
  }
  _getTogglePosition() {
    return this.panel.togglePosition;
  }
  _showToggle() {
    return !this.panel.hideToggle && !this.panel.disabled;
  }
  _getHeaderHeight() {
    const isExpanded = this._isExpanded();
    if (isExpanded && this.expandedHeight) {
      return this.expandedHeight;
    } else if (!isExpanded && this.collapsedHeight) {
      return this.collapsedHeight;
    }
    return null;
  }
  _keydown(event) {
    switch (event.keyCode) {
      case SPACE:
      case ENTER:
        if (!hasModifierKey(event)) {
          event.preventDefault();
          this._toggle();
        }
        break;
      default:
        if (this.panel.accordion) {
          this.panel.accordion._handleHeaderKeydown(event);
        }
        return;
    }
  }
  focus(origin, options) {
    if (origin) {
      this._focusMonitor.focusVia(this._element, origin, options);
    } else {
      this._element.nativeElement.focus(options);
    }
  }
  ngAfterViewInit() {
    this._focusMonitor.monitor(this._element).subscribe((origin) => {
      if (origin && this.panel.accordion) {
        this.panel.accordion._handleHeaderFocus(this);
      }
    });
  }
  ngOnDestroy() {
    this._parentChangeSubscription.unsubscribe();
    this._focusMonitor.stopMonitoring(this._element);
  }
  static ɵfac = function MatExpansionPanelHeader_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatExpansionPanelHeader)();
  };
  static ɵcmp = ɵɵdefineComponent({
    type: _MatExpansionPanelHeader,
    selectors: [["mat-expansion-panel-header"]],
    hostAttrs: ["role", "button", 1, "mat-expansion-panel-header", "mat-focus-indicator"],
    hostVars: 13,
    hostBindings: function MatExpansionPanelHeader_HostBindings(rf, ctx) {
      if (rf & 1) {
        ɵɵlistener("click", function MatExpansionPanelHeader_click_HostBindingHandler() {
          return ctx._toggle();
        })("keydown", function MatExpansionPanelHeader_keydown_HostBindingHandler($event) {
          return ctx._keydown($event);
        });
      }
      if (rf & 2) {
        ɵɵattribute("id", ctx.panel._headerId)("tabindex", ctx.disabled ? -1 : ctx.tabIndex)("aria-controls", ctx._getPanelId())("aria-expanded", ctx._isExpanded())("aria-disabled", ctx.panel.disabled);
        ɵɵstyleProp("height", ctx._getHeaderHeight());
        ɵɵclassProp("mat-expanded", ctx._isExpanded())("mat-expansion-toggle-indicator-after", ctx._getTogglePosition() === "after")("mat-expansion-toggle-indicator-before", ctx._getTogglePosition() === "before");
      }
    },
    inputs: {
      expandedHeight: "expandedHeight",
      collapsedHeight: "collapsedHeight",
      tabIndex: [2, "tabIndex", "tabIndex", (value) => value == null ? 0 : numberAttribute(value)]
    },
    ngContentSelectors: _c5,
    decls: 5,
    vars: 3,
    consts: [[1, "mat-content"], [1, "mat-expansion-indicator"], ["xmlns", "http://www.w3.org/2000/svg", "viewBox", "0 -960 960 960", "aria-hidden", "true", "focusable", "false"], ["d", "M480-345 240-585l56-56 184 184 184-184 56 56-240 240Z"]],
    template: function MatExpansionPanelHeader_Template(rf, ctx) {
      if (rf & 1) {
        ɵɵprojectionDef(_c4);
        ɵɵdomElementStart(0, "span", 0);
        ɵɵprojection(1);
        ɵɵprojection(2, 1);
        ɵɵprojection(3, 2);
        ɵɵdomElementEnd();
        ɵɵconditionalCreate(4, MatExpansionPanelHeader_Conditional_4_Template, 3, 0, "span", 1);
      }
      if (rf & 2) {
        ɵɵclassProp("mat-content-hide-toggle", !ctx._showToggle());
        ɵɵadvance(4);
        ɵɵconditional(ctx._showToggle() ? 4 : -1);
      }
    },
    styles: ['.mat-expansion-panel-header {\n  display: flex;\n  flex-direction: row;\n  align-items: center;\n  padding: 0 24px;\n  border-radius: inherit;\n}\n.mat-expansion-panel-animations-enabled .mat-expansion-panel-header {\n  transition: height 225ms cubic-bezier(0.4, 0, 0.2, 1);\n}\n.mat-expansion-panel-header::before {\n  border-radius: inherit;\n}\n.mat-expansion-panel-header {\n  height: var(--mat-expansion-header-collapsed-state-height, 48px);\n  font-family: var(--mat-expansion-header-text-font, var(--mat-sys-title-medium-font));\n  font-size: var(--mat-expansion-header-text-size, var(--mat-sys-title-medium-size));\n  font-weight: var(--mat-expansion-header-text-weight, var(--mat-sys-title-medium-weight));\n  line-height: var(--mat-expansion-header-text-line-height, var(--mat-sys-title-medium-line-height));\n  letter-spacing: var(--mat-expansion-header-text-tracking, var(--mat-sys-title-medium-tracking));\n}\n.mat-expansion-panel-header.mat-expanded {\n  height: var(--mat-expansion-header-expanded-state-height, 64px);\n}\n.mat-expansion-panel-header[aria-disabled=true] {\n  color: var(--mat-expansion-header-disabled-state-text-color, color-mix(in srgb, var(--mat-sys-on-surface) 38%, transparent));\n}\n.mat-expansion-panel-header:not([aria-disabled=true]) {\n  cursor: pointer;\n}\n.mat-expansion-panel:not(.mat-expanded) .mat-expansion-panel-header:not([aria-disabled=true]):hover {\n  background: var(--mat-expansion-header-hover-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-hover-state-layer-opacity) * 100%), transparent));\n}\n@media (hover: none) {\n  .mat-expansion-panel:not(.mat-expanded) .mat-expansion-panel-header:not([aria-disabled=true]):hover {\n    background: var(--mat-expansion-container-background-color, var(--mat-sys-surface));\n  }\n}\n.mat-expansion-panel .mat-expansion-panel-header:not([aria-disabled=true]).cdk-keyboard-focused, .mat-expansion-panel .mat-expansion-panel-header:not([aria-disabled=true]).cdk-program-focused {\n  background: var(--mat-expansion-header-focus-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-focus-state-layer-opacity) * 100%), transparent));\n}\n.mat-expansion-panel-header._mat-animation-noopable {\n  transition: none;\n}\n.mat-expansion-panel-header:focus, .mat-expansion-panel-header:hover {\n  outline: none;\n}\n.mat-expansion-panel-header.mat-expanded:focus, .mat-expansion-panel-header.mat-expanded:hover {\n  background: inherit;\n}\n.mat-expansion-panel-header.mat-expansion-toggle-indicator-before {\n  flex-direction: row-reverse;\n}\n.mat-expansion-panel-header.mat-expansion-toggle-indicator-before .mat-expansion-indicator {\n  margin: 0 16px 0 0;\n}\n[dir=rtl] .mat-expansion-panel-header.mat-expansion-toggle-indicator-before .mat-expansion-indicator {\n  margin: 0 0 0 16px;\n}\n\n.mat-content {\n  display: flex;\n  flex: 1;\n  flex-direction: row;\n  overflow: hidden;\n}\n.mat-content.mat-content-hide-toggle {\n  margin-right: 8px;\n}\n[dir=rtl] .mat-content.mat-content-hide-toggle {\n  margin-right: 0;\n  margin-left: 8px;\n}\n.mat-expansion-toggle-indicator-before .mat-content.mat-content-hide-toggle {\n  margin-left: 24px;\n  margin-right: 0;\n}\n[dir=rtl] .mat-expansion-toggle-indicator-before .mat-content.mat-content-hide-toggle {\n  margin-right: 24px;\n  margin-left: 0;\n}\n\n.mat-expansion-panel-header-title {\n  color: var(--mat-expansion-header-text-color, var(--mat-sys-on-surface));\n}\n\n.mat-expansion-panel-header-title,\n.mat-expansion-panel-header-description {\n  display: flex;\n  flex-grow: 1;\n  flex-basis: 0;\n  margin-right: 16px;\n  align-items: center;\n}\n[dir=rtl] .mat-expansion-panel-header-title,\n[dir=rtl] .mat-expansion-panel-header-description {\n  margin-right: 0;\n  margin-left: 16px;\n}\n.mat-expansion-panel-header[aria-disabled=true] .mat-expansion-panel-header-title,\n.mat-expansion-panel-header[aria-disabled=true] .mat-expansion-panel-header-description {\n  color: inherit;\n}\n\n.mat-expansion-panel-header-description {\n  flex-grow: 2;\n  color: var(--mat-expansion-header-description-color, var(--mat-sys-on-surface-variant));\n}\n\n.mat-expansion-panel-animations-enabled .mat-expansion-indicator {\n  transition: transform 225ms cubic-bezier(0.4, 0, 0.2, 1);\n}\n.mat-expansion-panel-header.mat-expanded .mat-expansion-indicator {\n  transform: rotate(180deg);\n}\n.mat-expansion-indicator::after {\n  border-style: solid;\n  border-width: 0 2px 2px 0;\n  content: "";\n  padding: 3px;\n  transform: rotate(45deg);\n  vertical-align: middle;\n  color: var(--mat-expansion-header-indicator-color, var(--mat-sys-on-surface-variant));\n  display: var(--mat-expansion-legacy-header-indicator-display, none);\n}\n.mat-expansion-indicator svg {\n  width: 24px;\n  height: 24px;\n  margin: 0 -8px;\n  vertical-align: middle;\n  fill: var(--mat-expansion-header-indicator-color, var(--mat-sys-on-surface-variant));\n  display: var(--mat-expansion-header-indicator-display, inline-block);\n}\n\n@media (forced-colors: active) {\n  .mat-expansion-panel-content {\n    border-top: 1px solid;\n    border-top-left-radius: 0;\n    border-top-right-radius: 0;\n  }\n}\n'],
    encapsulation: 2,
    changeDetection: 0
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatExpansionPanelHeader, [{
    type: Component,
    args: [{
      selector: "mat-expansion-panel-header",
      encapsulation: ViewEncapsulation.None,
      changeDetection: ChangeDetectionStrategy.OnPush,
      host: {
        "class": "mat-expansion-panel-header mat-focus-indicator",
        "role": "button",
        "[attr.id]": "panel._headerId",
        "[attr.tabindex]": "disabled ? -1 : tabIndex",
        "[attr.aria-controls]": "_getPanelId()",
        "[attr.aria-expanded]": "_isExpanded()",
        "[attr.aria-disabled]": "panel.disabled",
        "[class.mat-expanded]": "_isExpanded()",
        "[class.mat-expansion-toggle-indicator-after]": `_getTogglePosition() === 'after'`,
        "[class.mat-expansion-toggle-indicator-before]": `_getTogglePosition() === 'before'`,
        "[style.height]": "_getHeaderHeight()",
        "(click)": "_toggle()",
        "(keydown)": "_keydown($event)"
      },
      template: '<span class="mat-content" [class.mat-content-hide-toggle]="!_showToggle()">\n  <ng-content select="mat-panel-title"></ng-content>\n  <ng-content select="mat-panel-description"></ng-content>\n  <ng-content></ng-content>\n</span>\n\n@if (_showToggle()) {\n  <span class="mat-expansion-indicator">\n    <svg\n      xmlns="http://www.w3.org/2000/svg"\n      viewBox="0 -960 960 960"\n      aria-hidden="true"\n      focusable="false">\n      <path d="M480-345 240-585l56-56 184 184 184-184 56 56-240 240Z"/>\n    </svg>\n  </span>\n}\n',
      styles: ['.mat-expansion-panel-header {\n  display: flex;\n  flex-direction: row;\n  align-items: center;\n  padding: 0 24px;\n  border-radius: inherit;\n}\n.mat-expansion-panel-animations-enabled .mat-expansion-panel-header {\n  transition: height 225ms cubic-bezier(0.4, 0, 0.2, 1);\n}\n.mat-expansion-panel-header::before {\n  border-radius: inherit;\n}\n.mat-expansion-panel-header {\n  height: var(--mat-expansion-header-collapsed-state-height, 48px);\n  font-family: var(--mat-expansion-header-text-font, var(--mat-sys-title-medium-font));\n  font-size: var(--mat-expansion-header-text-size, var(--mat-sys-title-medium-size));\n  font-weight: var(--mat-expansion-header-text-weight, var(--mat-sys-title-medium-weight));\n  line-height: var(--mat-expansion-header-text-line-height, var(--mat-sys-title-medium-line-height));\n  letter-spacing: var(--mat-expansion-header-text-tracking, var(--mat-sys-title-medium-tracking));\n}\n.mat-expansion-panel-header.mat-expanded {\n  height: var(--mat-expansion-header-expanded-state-height, 64px);\n}\n.mat-expansion-panel-header[aria-disabled=true] {\n  color: var(--mat-expansion-header-disabled-state-text-color, color-mix(in srgb, var(--mat-sys-on-surface) 38%, transparent));\n}\n.mat-expansion-panel-header:not([aria-disabled=true]) {\n  cursor: pointer;\n}\n.mat-expansion-panel:not(.mat-expanded) .mat-expansion-panel-header:not([aria-disabled=true]):hover {\n  background: var(--mat-expansion-header-hover-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-hover-state-layer-opacity) * 100%), transparent));\n}\n@media (hover: none) {\n  .mat-expansion-panel:not(.mat-expanded) .mat-expansion-panel-header:not([aria-disabled=true]):hover {\n    background: var(--mat-expansion-container-background-color, var(--mat-sys-surface));\n  }\n}\n.mat-expansion-panel .mat-expansion-panel-header:not([aria-disabled=true]).cdk-keyboard-focused, .mat-expansion-panel .mat-expansion-panel-header:not([aria-disabled=true]).cdk-program-focused {\n  background: var(--mat-expansion-header-focus-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-focus-state-layer-opacity) * 100%), transparent));\n}\n.mat-expansion-panel-header._mat-animation-noopable {\n  transition: none;\n}\n.mat-expansion-panel-header:focus, .mat-expansion-panel-header:hover {\n  outline: none;\n}\n.mat-expansion-panel-header.mat-expanded:focus, .mat-expansion-panel-header.mat-expanded:hover {\n  background: inherit;\n}\n.mat-expansion-panel-header.mat-expansion-toggle-indicator-before {\n  flex-direction: row-reverse;\n}\n.mat-expansion-panel-header.mat-expansion-toggle-indicator-before .mat-expansion-indicator {\n  margin: 0 16px 0 0;\n}\n[dir=rtl] .mat-expansion-panel-header.mat-expansion-toggle-indicator-before .mat-expansion-indicator {\n  margin: 0 0 0 16px;\n}\n\n.mat-content {\n  display: flex;\n  flex: 1;\n  flex-direction: row;\n  overflow: hidden;\n}\n.mat-content.mat-content-hide-toggle {\n  margin-right: 8px;\n}\n[dir=rtl] .mat-content.mat-content-hide-toggle {\n  margin-right: 0;\n  margin-left: 8px;\n}\n.mat-expansion-toggle-indicator-before .mat-content.mat-content-hide-toggle {\n  margin-left: 24px;\n  margin-right: 0;\n}\n[dir=rtl] .mat-expansion-toggle-indicator-before .mat-content.mat-content-hide-toggle {\n  margin-right: 24px;\n  margin-left: 0;\n}\n\n.mat-expansion-panel-header-title {\n  color: var(--mat-expansion-header-text-color, var(--mat-sys-on-surface));\n}\n\n.mat-expansion-panel-header-title,\n.mat-expansion-panel-header-description {\n  display: flex;\n  flex-grow: 1;\n  flex-basis: 0;\n  margin-right: 16px;\n  align-items: center;\n}\n[dir=rtl] .mat-expansion-panel-header-title,\n[dir=rtl] .mat-expansion-panel-header-description {\n  margin-right: 0;\n  margin-left: 16px;\n}\n.mat-expansion-panel-header[aria-disabled=true] .mat-expansion-panel-header-title,\n.mat-expansion-panel-header[aria-disabled=true] .mat-expansion-panel-header-description {\n  color: inherit;\n}\n\n.mat-expansion-panel-header-description {\n  flex-grow: 2;\n  color: var(--mat-expansion-header-description-color, var(--mat-sys-on-surface-variant));\n}\n\n.mat-expansion-panel-animations-enabled .mat-expansion-indicator {\n  transition: transform 225ms cubic-bezier(0.4, 0, 0.2, 1);\n}\n.mat-expansion-panel-header.mat-expanded .mat-expansion-indicator {\n  transform: rotate(180deg);\n}\n.mat-expansion-indicator::after {\n  border-style: solid;\n  border-width: 0 2px 2px 0;\n  content: "";\n  padding: 3px;\n  transform: rotate(45deg);\n  vertical-align: middle;\n  color: var(--mat-expansion-header-indicator-color, var(--mat-sys-on-surface-variant));\n  display: var(--mat-expansion-legacy-header-indicator-display, none);\n}\n.mat-expansion-indicator svg {\n  width: 24px;\n  height: 24px;\n  margin: 0 -8px;\n  vertical-align: middle;\n  fill: var(--mat-expansion-header-indicator-color, var(--mat-sys-on-surface-variant));\n  display: var(--mat-expansion-header-indicator-display, inline-block);\n}\n\n@media (forced-colors: active) {\n  .mat-expansion-panel-content {\n    border-top: 1px solid;\n    border-top-left-radius: 0;\n    border-top-right-radius: 0;\n  }\n}\n']
    }]
  }], () => [], {
    expandedHeight: [{
      type: Input
    }],
    collapsedHeight: [{
      type: Input
    }],
    tabIndex: [{
      type: Input,
      args: [{
        transform: (value) => value == null ? 0 : numberAttribute(value)
      }]
    }]
  });
})();
var MatExpansionPanelDescription = class _MatExpansionPanelDescription {
  static ɵfac = function MatExpansionPanelDescription_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatExpansionPanelDescription)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _MatExpansionPanelDescription,
    selectors: [["mat-panel-description"]],
    hostAttrs: [1, "mat-expansion-panel-header-description"]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatExpansionPanelDescription, [{
    type: Directive,
    args: [{
      selector: "mat-panel-description",
      host: {
        class: "mat-expansion-panel-header-description"
      }
    }]
  }], null, null);
})();
var MatExpansionPanelTitle = class _MatExpansionPanelTitle {
  static ɵfac = function MatExpansionPanelTitle_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatExpansionPanelTitle)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _MatExpansionPanelTitle,
    selectors: [["mat-panel-title"]],
    hostAttrs: [1, "mat-expansion-panel-header-title"]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatExpansionPanelTitle, [{
    type: Directive,
    args: [{
      selector: "mat-panel-title",
      host: {
        class: "mat-expansion-panel-header-title"
      }
    }]
  }], null, null);
})();
var MatAccordion = class _MatAccordion extends CdkAccordion {
  _keyManager;
  _ownHeaders = new QueryList();
  _headers;
  hideToggle = false;
  displayMode = "default";
  togglePosition = "after";
  ngAfterContentInit() {
    this._headers.changes.pipe(startWith(this._headers)).subscribe((headers) => {
      this._ownHeaders.reset(headers.filter((header) => header.panel.accordion === this));
      this._ownHeaders.notifyOnChanges();
    });
    this._keyManager = new FocusKeyManager(this._ownHeaders).withWrap().withHomeAndEnd();
  }
  _handleHeaderKeydown(event) {
    this._keyManager.onKeydown(event);
  }
  _handleHeaderFocus(header) {
    this._keyManager.updateActiveItem(header);
  }
  ngOnDestroy() {
    super.ngOnDestroy();
    this._keyManager?.destroy();
    this._ownHeaders.destroy();
  }
  static ɵfac = /* @__PURE__ */ (() => {
    let ɵMatAccordion_BaseFactory;
    return function MatAccordion_Factory(__ngFactoryType__) {
      return (ɵMatAccordion_BaseFactory || (ɵMatAccordion_BaseFactory = ɵɵgetInheritedFactory(_MatAccordion)))(__ngFactoryType__ || _MatAccordion);
    };
  })();
  static ɵdir = ɵɵdefineDirective({
    type: _MatAccordion,
    selectors: [["mat-accordion"]],
    contentQueries: function MatAccordion_ContentQueries(rf, ctx, dirIndex) {
      if (rf & 1) {
        ɵɵcontentQuery(dirIndex, MatExpansionPanelHeader, 5);
      }
      if (rf & 2) {
        let _t;
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._headers = _t);
      }
    },
    hostAttrs: [1, "mat-accordion"],
    hostVars: 2,
    hostBindings: function MatAccordion_HostBindings(rf, ctx) {
      if (rf & 2) {
        ɵɵclassProp("mat-accordion-multi", ctx.multi);
      }
    },
    inputs: {
      hideToggle: [2, "hideToggle", "hideToggle", booleanAttribute],
      displayMode: "displayMode",
      togglePosition: "togglePosition"
    },
    exportAs: ["matAccordion"],
    features: [ɵɵProvidersFeature([{
      provide: MAT_ACCORDION,
      useExisting: _MatAccordion
    }]), ɵɵInheritDefinitionFeature]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatAccordion, [{
    type: Directive,
    args: [{
      selector: "mat-accordion",
      exportAs: "matAccordion",
      providers: [{
        provide: MAT_ACCORDION,
        useExisting: MatAccordion
      }],
      host: {
        class: "mat-accordion",
        "[class.mat-accordion-multi]": "this.multi"
      }
    }]
  }], null, {
    _headers: [{
      type: ContentChildren,
      args: [MatExpansionPanelHeader, {
        descendants: true
      }]
    }],
    hideToggle: [{
      type: Input,
      args: [{
        transform: booleanAttribute
      }]
    }],
    displayMode: [{
      type: Input
    }],
    togglePosition: [{
      type: Input
    }]
  });
})();
var MatExpansionModule = class _MatExpansionModule {
  static ɵfac = function MatExpansionModule_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatExpansionModule)();
  };
  static ɵmod = ɵɵdefineNgModule({
    type: _MatExpansionModule,
    imports: [CdkAccordionModule, PortalModule, MatAccordion, MatExpansionPanel, MatExpansionPanelActionRow, MatExpansionPanelHeader, MatExpansionPanelTitle, MatExpansionPanelDescription, MatExpansionPanelContent],
    exports: [BidiModule, MatAccordion, MatExpansionPanel, MatExpansionPanelActionRow, MatExpansionPanelHeader, MatExpansionPanelTitle, MatExpansionPanelDescription, MatExpansionPanelContent]
  });
  static ɵinj = ɵɵdefineInjector({
    imports: [CdkAccordionModule, PortalModule, BidiModule]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatExpansionModule, [{
    type: NgModule,
    args: [{
      imports: [CdkAccordionModule, PortalModule, MatAccordion, MatExpansionPanel, MatExpansionPanelActionRow, MatExpansionPanelHeader, MatExpansionPanelTitle, MatExpansionPanelDescription, MatExpansionPanelContent],
      exports: [BidiModule, MatAccordion, MatExpansionPanel, MatExpansionPanelActionRow, MatExpansionPanelHeader, MatExpansionPanelTitle, MatExpansionPanelDescription, MatExpansionPanelContent]
    }]
  }], null, null);
})();
export {
  MAT_ACCORDION,
  MAT_EXPANSION_PANEL,
  MAT_EXPANSION_PANEL_DEFAULT_OPTIONS,
  MatAccordion,
  MatExpansionModule,
  MatExpansionPanel,
  MatExpansionPanelActionRow,
  MatExpansionPanelContent,
  MatExpansionPanelDescription,
  MatExpansionPanelHeader,
  MatExpansionPanelTitle
};
//# sourceMappingURL=@angular_material_expansion.js.map
