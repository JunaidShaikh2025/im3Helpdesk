import {
  DomSanitizer
} from "./chunk-ENC6VFIZ.js";
import "./chunk-B4BO3VX5.js";
import "./chunk-FDV2MSRL.js";
import "./chunk-YO2PVG56.js";
import "./chunk-NPEN54ZG.js";
import {
  ApplicationRef,
  ChangeDetectionStrategy,
  Component,
  DOCUMENT,
  Directive,
  ElementRef,
  Injectable,
  InjectionToken,
  Injector,
  NgModule,
  NgZone,
  SecurityContext,
  computed,
  createComponent,
  inject,
  linkedSignal,
  makeEnvironmentProviders,
  setClassMetadata,
  signal,
  ɵɵInheritDefinitionFeature,
  ɵɵadvance,
  ɵɵanimateEnter,
  ɵɵattribute,
  ɵɵclassMap,
  ɵɵconditional,
  ɵɵconditionalCreate,
  ɵɵdeclareLet,
  ɵɵdefineComponent,
  ɵɵdefineDirective,
  ɵɵdefineInjectable,
  ɵɵdefineInjector,
  ɵɵdefineNgModule,
  ɵɵdomElement,
  ɵɵdomElementContainerEnd,
  ɵɵdomElementContainerStart,
  ɵɵdomElementEnd,
  ɵɵdomElementStart,
  ɵɵdomListener,
  ɵɵdomProperty,
  ɵɵgetCurrentView,
  ɵɵgetInheritedFactory,
  ɵɵlistener,
  ɵɵnextContext,
  ɵɵreadContextLet,
  ɵɵresetView,
  ɵɵrestoreView,
  ɵɵsanitizeHtml,
  ɵɵstoreLet,
  ɵɵstyleProp,
  ɵɵtext,
  ɵɵtextInterpolate1
} from "./chunk-3BW4HTRP.js";
import "./chunk-HWYXSU2G.js";
import "./chunk-JRFR6BLO.js";
import {
  Subject
} from "./chunk-MARUHEWW.js";
import {
  __spreadProps,
  __spreadValues
} from "./chunk-XSXJULYR.js";

// node_modules/ngx-toastr/fesm2022/ngx-toastr.mjs
var _c0 = ["toast-component", ""];
function ToastBase_Conditional_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r1 = ɵɵgetCurrentView();
    ɵɵdomElementStart(0, "button", 2);
    ɵɵdomListener("click", function ToastBase_Conditional_1_Template_button_click_0_listener() {
      ɵɵrestoreView(_r1);
      const ctx_r1 = ɵɵnextContext();
      return ɵɵresetView(ctx_r1.remove());
    });
    ɵɵdomElementStart(1, "span", 3);
    ɵɵtext(2, "×");
    ɵɵdomElementEnd()();
  }
}
function ToastBase_Conditional_2_Conditional_2_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵdomElementContainerStart(0);
    ɵɵtext(1);
    ɵɵdomElementContainerEnd();
  }
  if (rf & 2) {
    const ctx_r1 = ɵɵnextContext(2);
    ɵɵadvance();
    ɵɵtextInterpolate1("[", ctx_r1.duplicatesCount + 1, "]");
  }
}
function ToastBase_Conditional_2_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵdomElementStart(0, "div");
    ɵɵtext(1);
    ɵɵconditionalCreate(2, ToastBase_Conditional_2_Conditional_2_Template, 2, 1, "ng-container");
    ɵɵdomElementEnd();
  }
  if (rf & 2) {
    const ctx_r1 = ɵɵnextContext();
    const _options_r3 = ɵɵreadContextLet(0);
    ɵɵclassMap(_options_r3.titleClass);
    ɵɵattribute("aria-label", ctx_r1.title());
    ɵɵadvance();
    ɵɵtextInterpolate1(" ", ctx_r1.title(), " ");
    ɵɵadvance();
    ɵɵconditional(ctx_r1.duplicatesCount ? 2 : -1);
  }
}
function ToastBase_Conditional_3_Conditional_0_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵdomElement(0, "div", 6);
  }
  if (rf & 2) {
    const ctx_r1 = ɵɵnextContext(2);
    const _options_r3 = ɵɵreadContextLet(0);
    ɵɵclassMap(_options_r3.messageClass);
    ɵɵdomProperty("innerHTML", ctx_r1.message(), ɵɵsanitizeHtml);
  }
}
function ToastBase_Conditional_3_Conditional_1_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵdomElementStart(0, "div", 7);
    ɵɵtext(1);
    ɵɵdomElementEnd();
  }
  if (rf & 2) {
    const ctx_r1 = ɵɵnextContext(2);
    const _options_r3 = ɵɵreadContextLet(0);
    ɵɵclassMap(_options_r3.messageClass);
    ɵɵattribute("aria-label", ctx_r1.message());
    ɵɵadvance();
    ɵɵtextInterpolate1(" ", ctx_r1.message(), " ");
  }
}
function ToastBase_Conditional_3_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵconditionalCreate(0, ToastBase_Conditional_3_Conditional_0_Template, 1, 3, "div", 4)(1, ToastBase_Conditional_3_Conditional_1_Template, 2, 4, "div", 5);
  }
  if (rf & 2) {
    ɵɵnextContext();
    const _options_r3 = ɵɵreadContextLet(0);
    ɵɵconditional(_options_r3.enableHtml ? 0 : 1);
  }
}
function ToastBase_Conditional_4_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵdomElementStart(0, "div");
    ɵɵdomElement(1, "div", 8);
    ɵɵdomElementEnd();
  }
  if (rf & 2) {
    const ctx_r1 = ɵɵnextContext();
    ɵɵadvance();
    ɵɵstyleProp("width", ctx_r1.width() + "%");
  }
}
function Toast_Conditional_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r1 = ɵɵgetCurrentView();
    ɵɵdomElementStart(0, "button", 2);
    ɵɵdomListener("click", function Toast_Conditional_1_Template_button_click_0_listener() {
      ɵɵrestoreView(_r1);
      const ctx_r1 = ɵɵnextContext();
      return ɵɵresetView(ctx_r1.remove());
    });
    ɵɵdomElementStart(1, "span", 3);
    ɵɵtext(2, "×");
    ɵɵdomElementEnd()();
  }
}
function Toast_Conditional_2_Conditional_2_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵdomElementContainerStart(0);
    ɵɵtext(1);
    ɵɵdomElementContainerEnd();
  }
  if (rf & 2) {
    const ctx_r1 = ɵɵnextContext(2);
    ɵɵadvance();
    ɵɵtextInterpolate1("[", ctx_r1.duplicatesCount + 1, "]");
  }
}
function Toast_Conditional_2_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵdomElementStart(0, "div");
    ɵɵtext(1);
    ɵɵconditionalCreate(2, Toast_Conditional_2_Conditional_2_Template, 2, 1, "ng-container");
    ɵɵdomElementEnd();
  }
  if (rf & 2) {
    const ctx_r1 = ɵɵnextContext();
    const _options_r3 = ɵɵreadContextLet(0);
    ɵɵclassMap(_options_r3.titleClass);
    ɵɵattribute("aria-label", ctx_r1.title());
    ɵɵadvance();
    ɵɵtextInterpolate1(" ", ctx_r1.title(), " ");
    ɵɵadvance();
    ɵɵconditional(ctx_r1.duplicatesCount ? 2 : -1);
  }
}
function Toast_Conditional_3_Conditional_0_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵdomElement(0, "div", 6);
  }
  if (rf & 2) {
    const ctx_r1 = ɵɵnextContext(2);
    const _options_r3 = ɵɵreadContextLet(0);
    ɵɵclassMap(_options_r3.messageClass);
    ɵɵdomProperty("innerHTML", ctx_r1.message(), ɵɵsanitizeHtml);
  }
}
function Toast_Conditional_3_Conditional_1_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵdomElementStart(0, "div", 7);
    ɵɵtext(1);
    ɵɵdomElementEnd();
  }
  if (rf & 2) {
    const ctx_r1 = ɵɵnextContext(2);
    const _options_r3 = ɵɵreadContextLet(0);
    ɵɵclassMap(_options_r3.messageClass);
    ɵɵattribute("aria-label", ctx_r1.message());
    ɵɵadvance();
    ɵɵtextInterpolate1(" ", ctx_r1.message(), " ");
  }
}
function Toast_Conditional_3_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵconditionalCreate(0, Toast_Conditional_3_Conditional_0_Template, 1, 3, "div", 4)(1, Toast_Conditional_3_Conditional_1_Template, 2, 4, "div", 5);
  }
  if (rf & 2) {
    ɵɵnextContext();
    const _options_r3 = ɵɵreadContextLet(0);
    ɵɵconditional(_options_r3.enableHtml ? 0 : 1);
  }
}
function Toast_Conditional_4_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵdomElementStart(0, "div");
    ɵɵdomElement(1, "div", 8);
    ɵɵdomElementEnd();
  }
  if (rf & 2) {
    const ctx_r1 = ɵɵnextContext();
    ɵɵadvance();
    ɵɵstyleProp("width", ctx_r1.width() + "%");
  }
}
var ToastContainerDirective = class _ToastContainerDirective {
  el = inject(ElementRef);
  getContainerElement() {
    return this.el.nativeElement;
  }
  static ɵfac = function ToastContainerDirective_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _ToastContainerDirective)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _ToastContainerDirective,
    selectors: [["", "toastContainer", ""]],
    exportAs: ["toastContainer"]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(ToastContainerDirective, [{
    type: Directive,
    args: [{
      selector: "[toastContainer]",
      exportAs: "toastContainer",
      standalone: true
    }]
  }], null, null);
})();
var ToastPackage = class {
  toastId;
  config;
  message;
  title;
  toastType;
  toastRef;
  _onTap = new Subject();
  _onAction = new Subject();
  constructor(toastId, config, message, title, toastType, toastRef) {
    this.toastId = toastId;
    this.config = config;
    this.message = message;
    this.title = title;
    this.toastType = toastType;
    this.toastRef = toastRef;
    this.toastRef.afterClosed().subscribe(() => {
      this._onAction.complete();
      this._onTap.complete();
    });
  }
  /** Fired on click */
  triggerTap() {
    this._onTap.next();
    if (this.config.tapToDismiss) {
      this._onTap.complete();
    }
  }
  onTap() {
    return this._onTap.asObservable();
  }
  /** available for use in custom toast */
  triggerAction(action) {
    this._onAction.next(action);
  }
  onAction() {
    return this._onAction.asObservable();
  }
};
var DefaultNoComponentGlobalConfig = {
  maxOpened: 0,
  autoDismiss: false,
  newestOnTop: true,
  preventDuplicates: false,
  countDuplicates: false,
  resetTimeoutOnDuplicate: false,
  includeTitleDuplicates: false,
  iconClasses: {
    error: "toast-error",
    info: "toast-info",
    success: "toast-success",
    warning: "toast-warning"
  },
  // Individual
  closeButton: false,
  disableTimeOut: false,
  timeOut: 5e3,
  extendedTimeOut: 1e3,
  enableHtml: false,
  progressBar: false,
  toastClass: "ngx-toastr",
  positionClass: "toast-top-right",
  titleClass: "toast-title",
  messageClass: "toast-message",
  easing: "ease-in",
  easeTime: 300,
  tapToDismiss: true,
  onActivateTick: false,
  progressAnimation: "decreasing"
};
var TOAST_CONFIG = new InjectionToken("ToastConfig");
var ComponentPortal = class {
  _attachedHost;
  /** The type of the component that will be instantiated for attachment. */
  component;
  /**
   * [Optional] Where the attached component should live in Angular's *logical* component tree.
   * This is different from where the component *renders*, which is determined by the PortalHost.
   * The origin necessary when the host is outside of the Angular application context.
   */
  viewContainerRef;
  /** Injector used for the instantiation of the component. */
  injector;
  constructor(component, injector) {
    this.component = component;
    this.injector = injector;
  }
  /** Attach this portal to a host. */
  attach(host, newestOnTop) {
    this._attachedHost = host;
    return host.attach(this, newestOnTop);
  }
  /** Detach this portal from its host */
  detach() {
    const host = this._attachedHost;
    if (host) {
      this._attachedHost = void 0;
      return host.detach();
    }
  }
  /** Whether this portal is attached to a host. */
  get isAttached() {
    return this._attachedHost != null;
  }
  /**
   * Sets the PortalHost reference without performing `attach()`. This is used directly by
   * the PortalHost when it is performing an `attach()` or `detach()`.
   */
  setAttachedHost(host) {
    this._attachedHost = host;
  }
};
var BasePortalHost = class {
  /** The portal currently attached to the host. */
  _attachedPortal;
  /** A function that will permanently dispose this host. */
  _disposeFn;
  attach(portal, newestOnTop) {
    this._attachedPortal = portal;
    return this.attachComponentPortal(portal, newestOnTop);
  }
  detach() {
    if (this._attachedPortal) {
      this._attachedPortal.setAttachedHost();
    }
    this._attachedPortal = void 0;
    if (this._disposeFn) {
      this._disposeFn();
      this._disposeFn = void 0;
    }
  }
  setDisposeFn(fn) {
    this._disposeFn = fn;
  }
};
var DomPortalHost = class extends BasePortalHost {
  _hostDomElement;
  _appRef;
  constructor(_hostDomElement, _appRef) {
    super();
    this._hostDomElement = _hostDomElement;
    this._appRef = _appRef;
  }
  /**
   * Attach the given ComponentPortal to DOM element using the ComponentFactoryResolver.
   * @param portal Portal to be attached
   */
  attachComponentPortal(portal, newestOnTop) {
    const componentRef = createComponent(portal.component, {
      environmentInjector: this._appRef.injector,
      elementInjector: portal.injector
    });
    this._appRef.attachView(componentRef.hostView);
    this.setDisposeFn(() => {
      this._appRef.detachView(componentRef.hostView);
      componentRef.destroy();
    });
    if (newestOnTop) {
      this._hostDomElement.insertBefore(this._getComponentRootNode(componentRef), this._hostDomElement.firstChild);
    } else {
      this._hostDomElement.appendChild(this._getComponentRootNode(componentRef));
    }
    return componentRef;
  }
  /** Gets the root HTMLElement for an instantiated component. */
  _getComponentRootNode(componentRef) {
    return componentRef.hostView.rootNodes[0];
  }
};
var OverlayContainer = class _OverlayContainer {
  _document = inject(DOCUMENT);
  _containerElement;
  ngOnDestroy() {
    if (this._containerElement && this._containerElement.parentNode) {
      this._containerElement.parentNode.removeChild(this._containerElement);
    }
  }
  /**
   * This method returns the overlay container element. It will lazily
   * create the element the first time  it is called to facilitate using
   * the container in non-browser environments.
   * @returns the container element
   */
  getContainerElement() {
    if (!this._containerElement) {
      this._createContainer();
    }
    return this._containerElement;
  }
  /**
   * Create the overlay container element, which is simply a div
   * with the 'cdk-overlay-container' class on the document body
   * and 'aria-live="polite"'
   */
  _createContainer() {
    const container = this._document.createElement("div");
    container.classList.add("overlay-container");
    container.setAttribute("aria-live", "polite");
    this._document.body.appendChild(container);
    this._containerElement = container;
  }
  static ɵfac = function OverlayContainer_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _OverlayContainer)();
  };
  static ɵprov = ɵɵdefineInjectable({
    token: _OverlayContainer,
    factory: _OverlayContainer.ɵfac,
    providedIn: "root"
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(OverlayContainer, [{
    type: Injectable,
    args: [{
      providedIn: "root"
    }]
  }], null, null);
})();
var OverlayRef = class {
  _portalHost;
  constructor(_portalHost) {
    this._portalHost = _portalHost;
  }
  attach(portal, newestOnTop = true) {
    return this._portalHost.attach(portal, newestOnTop);
  }
  /**
   * Detaches an overlay from a portal.
   * @returns Resolves when the overlay has been detached.
   */
  detach() {
    return this._portalHost.detach();
  }
};
var Overlay = class _Overlay {
  _overlayContainer = inject(OverlayContainer);
  _appRef = inject(ApplicationRef);
  _document = inject(DOCUMENT);
  // Namespace panes by overlay container
  _paneElements = /* @__PURE__ */ new Map();
  /**
   * Creates an overlay.
   * @returns A reference to the created overlay.
   */
  create(positionClass, overlayContainer) {
    return this._createOverlayRef(this.getPaneElement(positionClass, overlayContainer));
  }
  getPaneElement(positionClass = "", overlayContainer) {
    if (!this._paneElements.get(overlayContainer)) {
      this._paneElements.set(overlayContainer, {});
    }
    if (!this._paneElements.get(overlayContainer)[positionClass]) {
      this._paneElements.get(overlayContainer)[positionClass] = this._createPaneElement(positionClass, overlayContainer);
    }
    return this._paneElements.get(overlayContainer)[positionClass];
  }
  /**
   * Creates the DOM element for an overlay and appends it to the overlay container.
   * @returns Newly-created pane element
   */
  _createPaneElement(positionClass, overlayContainer) {
    const pane = this._document.createElement("div");
    pane.id = "toast-container";
    pane.classList.add(positionClass);
    pane.classList.add("toast-container");
    if (!overlayContainer) {
      this._overlayContainer.getContainerElement().appendChild(pane);
    } else {
      overlayContainer.getContainerElement().appendChild(pane);
    }
    return pane;
  }
  /**
   * Create a DomPortalHost into which the overlay content can be loaded.
   * @param pane The DOM element to turn into a portal host.
   * @returns A portal host for the given DOM element.
   */
  _createPortalHost(pane) {
    return new DomPortalHost(pane, this._appRef);
  }
  /**
   * Creates an OverlayRef for an overlay in the given DOM element.
   * @param pane DOM element for the overlay
   */
  _createOverlayRef(pane) {
    return new OverlayRef(this._createPortalHost(pane));
  }
  static ɵfac = function Overlay_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _Overlay)();
  };
  static ɵprov = ɵɵdefineInjectable({
    token: _Overlay,
    factory: _Overlay.ɵfac,
    providedIn: "root"
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(Overlay, [{
    type: Injectable,
    args: [{
      providedIn: "root"
    }]
  }], null, null);
})();
var ToastRef = class {
  _overlayRef;
  /** The instance of component opened into the toast. */
  componentInstance;
  /** Count of duplicates of this toast */
  duplicatesCount = 0;
  /** Subject for notifying the user that the toast has finished closing. */
  _afterClosed = new Subject();
  /** triggered when toast is activated */
  _activate = new Subject();
  /** notifies the toast that it should close before the timeout */
  _manualClose = new Subject();
  /** notifies the toast that it should reset the timeouts */
  _resetTimeout = new Subject();
  /** notifies the toast that it should count a duplicate toast */
  _countDuplicate = new Subject();
  constructor(_overlayRef) {
    this._overlayRef = _overlayRef;
  }
  manualClose() {
    this._manualClose.next();
    this._manualClose.complete();
  }
  manualClosed() {
    return this._manualClose.asObservable();
  }
  timeoutReset() {
    return this._resetTimeout.asObservable();
  }
  countDuplicate() {
    return this._countDuplicate.asObservable();
  }
  /**
   * Close the toast.
   */
  close() {
    this._overlayRef.detach();
    this._afterClosed.next();
    this._manualClose.next();
    this._afterClosed.complete();
    this._manualClose.complete();
    this._activate.complete();
    this._resetTimeout.complete();
    this._countDuplicate.complete();
  }
  /** Gets an observable that is notified when the toast is finished closing. */
  afterClosed() {
    return this._afterClosed.asObservable();
  }
  isInactive() {
    return this._activate.closed;
  }
  activate() {
    this._activate.next();
    this._activate.complete();
  }
  /** Gets an observable that is notified when the toast has started opening. */
  afterActivate() {
    return this._activate.asObservable();
  }
  /** Reset the toast timouts and count duplicates */
  onDuplicate(resetTimeout, countDuplicate) {
    if (resetTimeout) {
      this._resetTimeout.next();
    }
    if (countDuplicate) {
      this._countDuplicate.next(++this.duplicatesCount);
    }
  }
};
var ToastrService = class _ToastrService {
  overlay = inject(Overlay);
  _injector = inject(Injector);
  sanitizer = inject(DomSanitizer);
  ngZone = inject(NgZone);
  toastrConfig;
  currentlyActive = 0;
  toasts = [];
  overlayContainer;
  previousToastMessage;
  index = 0;
  constructor() {
    const token = inject(TOAST_CONFIG);
    this.toastrConfig = __spreadValues(__spreadValues({}, token.default), token.config);
    if (token.config.iconClasses) {
      this.toastrConfig.iconClasses = __spreadValues(__spreadValues({}, token.default.iconClasses), token.config.iconClasses);
    }
  }
  /** show toast */
  show(message, title, override = {}, type = "") {
    return this._preBuildNotification(type, message, title, this.applyConfig(override));
  }
  /** show successful toast */
  success(message, title, override = {}) {
    const type = this.toastrConfig.iconClasses.success || "";
    return this._preBuildNotification(type, message, title, this.applyConfig(override));
  }
  /** show error toast */
  error(message, title, override = {}) {
    const type = this.toastrConfig.iconClasses.error || "";
    return this._preBuildNotification(type, message, title, this.applyConfig(override));
  }
  /** show info toast */
  info(message, title, override = {}) {
    const type = this.toastrConfig.iconClasses.info || "";
    return this._preBuildNotification(type, message, title, this.applyConfig(override));
  }
  /** show warning toast */
  warning(message, title, override = {}) {
    const type = this.toastrConfig.iconClasses.warning || "";
    return this._preBuildNotification(type, message, title, this.applyConfig(override));
  }
  /**
   * Remove all or a single toast by id
   */
  clear(toastId) {
    for (const toast of this.toasts) {
      if (toastId !== void 0) {
        if (toast.toastId === toastId) {
          toast.toastRef.manualClose();
          return;
        }
      } else {
        toast.toastRef.manualClose();
      }
    }
  }
  /**
   * Remove and destroy a single toast by id
   */
  remove(toastId) {
    const found = this._findToast(toastId);
    if (!found) {
      return false;
    }
    found.activeToast.toastRef.close();
    this.toasts.splice(found.index, 1);
    this.currentlyActive = this.currentlyActive - 1;
    if (!this.toastrConfig.maxOpened || !this.toasts.length) {
      return false;
    }
    if (this.currentlyActive < this.toastrConfig.maxOpened && this.toasts[this.currentlyActive]) {
      const p = this.toasts[this.currentlyActive].toastRef;
      if (!p.isInactive()) {
        this.currentlyActive = this.currentlyActive + 1;
        p.activate();
      }
    }
    return true;
  }
  /**
   * Determines if toast message is already shown
   */
  findDuplicate(title = "", message = "", resetOnDuplicate, countDuplicates) {
    const {
      includeTitleDuplicates
    } = this.toastrConfig;
    for (const toast of this.toasts) {
      const hasDuplicateTitle = includeTitleDuplicates && toast.title === title;
      if ((!includeTitleDuplicates || hasDuplicateTitle) && toast.message === message) {
        toast.toastRef.onDuplicate(resetOnDuplicate, countDuplicates);
        return toast;
      }
    }
    return null;
  }
  /** create a clone of global config and apply individual settings */
  applyConfig(override = {}) {
    return __spreadValues(__spreadValues({}, this.toastrConfig), override);
  }
  /**
   * Find toast object by id
   */
  _findToast(toastId) {
    for (let i = 0; i < this.toasts.length; i++) {
      if (this.toasts[i].toastId === toastId) {
        return {
          index: i,
          activeToast: this.toasts[i]
        };
      }
    }
    return null;
  }
  /**
   * Determines the need to run inside angular's zone then builds the toast
   */
  _preBuildNotification(toastType, message, title, config) {
    if (config.onActivateTick) {
      return this.ngZone.run(() => this._buildNotification(toastType, message, title, config));
    }
    return this._buildNotification(toastType, message, title, config);
  }
  /**
   * Creates and attaches toast data to component
   * returns the active toast, or in case preventDuplicates is enabled the original/non-duplicate active toast.
   */
  _buildNotification(toastType, message, title, config) {
    if (!config.toastComponent) {
      throw new Error("toastComponent required");
    }
    const duplicate = this.findDuplicate(title, message, this.toastrConfig.resetTimeoutOnDuplicate && config.timeOut > 0, this.toastrConfig.countDuplicates);
    if ((this.toastrConfig.includeTitleDuplicates && title || message) && this.toastrConfig.preventDuplicates && duplicate !== null) {
      return duplicate;
    }
    this.previousToastMessage = message;
    let keepInactive = false;
    if (this.toastrConfig.maxOpened && this.currentlyActive >= this.toastrConfig.maxOpened) {
      keepInactive = true;
      if (this.toastrConfig.autoDismiss) {
        this.clear(this.toasts[0].toastId);
      }
    }
    const overlayRef = this.overlay.create(config.positionClass, this.overlayContainer);
    this.index = this.index + 1;
    let sanitizedMessage = message;
    if (message && config.enableHtml) {
      sanitizedMessage = this.sanitizer.sanitize(SecurityContext.HTML, message);
    }
    const toastRef = new ToastRef(overlayRef);
    const toastPackage = new ToastPackage(this.index, config, sanitizedMessage, title, toastType, toastRef);
    const providers = [{
      provide: ToastPackage,
      useValue: toastPackage
    }];
    const toastInjector = Injector.create({
      providers,
      parent: this._injector
    });
    const component = new ComponentPortal(config.toastComponent, toastInjector);
    const portal = overlayRef.attach(component, config.newestOnTop);
    toastRef.componentInstance = portal.instance;
    const ins = {
      toastId: this.index,
      title: title || "",
      message: message || "",
      toastRef,
      onShown: toastRef.afterActivate(),
      onHidden: toastRef.afterClosed(),
      onTap: toastPackage.onTap(),
      onAction: toastPackage.onAction(),
      portal
    };
    if (!keepInactive) {
      this.currentlyActive = this.currentlyActive + 1;
      setTimeout(() => {
        ins.toastRef.activate();
      });
    }
    this.toasts.push(ins);
    return ins;
  }
  static ɵfac = function ToastrService_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _ToastrService)();
  };
  static ɵprov = ɵɵdefineInjectable({
    token: _ToastrService,
    factory: _ToastrService.ɵfac,
    providedIn: "root"
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(ToastrService, [{
    type: Injectable,
    args: [{
      providedIn: "root"
    }]
  }], () => [], null);
})();
var TimeoutsService = class _TimeoutsService {
  ngZone = inject(NgZone);
  setInterval(func, timeout) {
    if (this.ngZone) {
      return this.ngZone.runOutsideAngular(() => window.setInterval(() => this.runInsideAngular(func), timeout));
    } else {
      return window.setInterval(() => func(), timeout);
    }
  }
  setTimeout(func, timeout) {
    if (this.ngZone) {
      return this.ngZone.runOutsideAngular(() => window.setTimeout(() => this.runInsideAngular(func), timeout));
    } else {
      return window.setTimeout(() => func(), timeout);
    }
  }
  runInsideAngular(func) {
    if (this.ngZone) {
      this.ngZone.run(() => func());
    } else {
      func();
    }
  }
  static ɵfac = function TimeoutsService_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _TimeoutsService)();
  };
  static ɵprov = ɵɵdefineInjectable({
    token: _TimeoutsService,
    factory: _TimeoutsService.ɵfac,
    providedIn: "root"
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(TimeoutsService, [{
    type: Injectable,
    args: [{
      providedIn: "root"
    }]
  }], null, null);
})();
var ToastBase = class _ToastBase {
  toastPackage = inject(ToastPackage);
  toastrService = inject(ToastrService);
  appRef = inject(ApplicationRef);
  timeoutsService = inject(TimeoutsService);
  duplicatesCount;
  hideTime;
  /** width of progress bar */
  width = signal(-1, ...ngDevMode ? [{
    debugName: "width"
  }] : []);
  state = signal("inactive", ...ngDevMode ? [{
    debugName: "state"
  }] : []);
  /** hides component when waiting to be displayed */
  displayStyle = computed(() => this.state() === "inactive" ? "none" : void 0, ...ngDevMode ? [{
    debugName: "displayStyle"
  }] : []);
  message = computed(() => this.toastPackage.message, ...ngDevMode ? [{
    debugName: "message"
  }] : []);
  title = computed(() => this.toastPackage.title, ...ngDevMode ? [{
    debugName: "title"
  }] : []);
  options = linkedSignal(() => this.toastPackage.config, ...ngDevMode ? [{
    debugName: "options"
  }] : []);
  originalTimeout = computed(() => this.toastPackage.config.timeOut, ...ngDevMode ? [{
    debugName: "originalTimeout"
  }] : []);
  toastClasses = computed(() => `${this.toastPackage.toastType} ${this.toastPackage.config.toastClass}`, ...ngDevMode ? [{
    debugName: "toastClasses"
  }] : []);
  timeout;
  intervalId;
  afterActivateSubscription;
  manualClosedSubscription;
  timeoutResetSubscription;
  countDuplicateSubscription;
  constructor() {
    this.afterActivateSubscription = this.toastPackage.toastRef.afterActivate().subscribe(() => {
      this.activateToast();
    });
    this.manualClosedSubscription = this.toastPackage.toastRef.manualClosed().subscribe(() => {
      this.remove();
    });
    this.timeoutResetSubscription = this.toastPackage.toastRef.timeoutReset().subscribe(() => {
      this.resetTimeout();
    });
    this.countDuplicateSubscription = this.toastPackage.toastRef.countDuplicate().subscribe((count) => {
      this.duplicatesCount = count;
    });
  }
  ngOnDestroy() {
    this.afterActivateSubscription.unsubscribe();
    this.manualClosedSubscription.unsubscribe();
    this.timeoutResetSubscription.unsubscribe();
    this.countDuplicateSubscription.unsubscribe();
    clearInterval(this.intervalId);
    clearTimeout(this.timeout);
  }
  /**
   * activates toast and sets timeout
   */
  activateToast() {
    const options = this.options();
    this.state.set("active");
    if (!(options.disableTimeOut === true || options.disableTimeOut === "timeOut") && options.timeOut) {
      this.timeout = this.timeoutsService.setTimeout(() => this.remove(), options.timeOut);
      this.hideTime = (/* @__PURE__ */ new Date()).getTime() + options.timeOut;
      if (options.progressBar) {
        this.intervalId = this.timeoutsService.setInterval(() => this.updateProgress(), 10);
      }
    }
  }
  /**
   * updates progress bar width
   */
  updateProgress() {
    const options = this.options();
    if (this.width() === 0 || this.width() === 100 || !options.timeOut) {
      return;
    }
    const now = (/* @__PURE__ */ new Date()).getTime();
    const remaining = this.hideTime - now;
    this.width.set(remaining / options.timeOut * 100);
    if (options.progressAnimation === "increasing") {
      this.width.update((width) => 100 - width);
    }
    if (this.width() <= 0) {
      this.width.set(0);
    }
    if (this.width() >= 100) {
      this.width.set(100);
    }
  }
  resetTimeout() {
    const options = this.options();
    clearTimeout(this.timeout);
    clearInterval(this.intervalId);
    this.state.set("active");
    this.options.update((options2) => __spreadProps(__spreadValues({}, options2), {
      timeOut: this.originalTimeout()
    }));
    this.timeout = this.timeoutsService.setTimeout(() => this.remove(), this.originalTimeout());
    this.hideTime = (/* @__PURE__ */ new Date()).getTime() + (this.originalTimeout() || 0);
    this.width.set(-1);
    if (options.progressBar) this.intervalId = this.timeoutsService.setInterval(() => this.updateProgress(), 10);
  }
  /**
   * tells toastrService to remove this toast after animation time
   */
  remove() {
    if (this.state() === "removed") return;
    clearTimeout(this.timeout);
    this.state.set("removed");
    this.timeout = this.timeoutsService.setTimeout(() => this.toastrService.remove(this.toastPackage.toastId));
  }
  tapToast() {
    if (this.state() === "removed") return;
    this.toastPackage.triggerTap();
    if (this.options().tapToDismiss) {
      this.remove();
    }
  }
  stickAround() {
    if (this.state() === "removed") return;
    if (this.options().disableTimeOut !== "extendedTimeOut") {
      clearTimeout(this.timeout);
      this.options.update((options) => __spreadProps(__spreadValues({}, options), {
        timeOut: 0
      }));
      this.hideTime = 0;
      clearInterval(this.intervalId);
      this.width.set(0);
    }
  }
  delayedHideToast() {
    const options = this.options();
    if (options.disableTimeOut === true || options.disableTimeOut === "extendedTimeOut" || options.extendedTimeOut === 0 || this.state() === "removed") {
      return;
    }
    const extendedTimeOut = options.extendedTimeOut;
    this.timeout = this.timeoutsService.setTimeout(() => this.remove(), extendedTimeOut);
    this.options.update((options2) => __spreadProps(__spreadValues({}, options2), {
      timeOut: extendedTimeOut
    }));
    this.hideTime = (/* @__PURE__ */ new Date()).getTime() + (extendedTimeOut || 0);
    this.width.set(-1);
    if (options.progressBar) {
      this.intervalId = this.timeoutsService.setInterval(() => this.updateProgress(), 10);
    }
  }
  static ɵfac = function ToastBase_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _ToastBase)();
  };
  static ɵcmp = ɵɵdefineComponent({
    type: _ToastBase,
    selectors: [["", "toast-component", ""]],
    hostVars: 4,
    hostBindings: function ToastBase_HostBindings(rf, ctx) {
      if (rf & 1) {
        ɵɵlistener("mouseenter", function ToastBase_mouseenter_HostBindingHandler() {
          return ctx.stickAround();
        })("mouseleave", function ToastBase_mouseleave_HostBindingHandler() {
          return ctx.delayedHideToast();
        })("click", function ToastBase_click_HostBindingHandler() {
          return ctx.tapToast();
        });
      }
      if (rf & 2) {
        ɵɵclassMap(ctx.toastClasses());
        ɵɵstyleProp("display", ctx.displayStyle());
      }
    },
    attrs: _c0,
    decls: 5,
    vars: 5,
    consts: [["type", "button", "aria-label", "Close", 1, "toast-close-button"], [3, "class"], ["type", "button", "aria-label", "Close", 1, "toast-close-button", 3, "click"], ["aria-hidden", "true"], ["role", "alert", 3, "class", "innerHTML"], ["role", "alert", 3, "class"], ["role", "alert", 3, "innerHTML"], ["role", "alert"], [1, "toast-progress"]],
    template: function ToastBase_Template(rf, ctx) {
      if (rf & 1) {
        ɵɵdeclareLet(0);
        ɵɵconditionalCreate(1, ToastBase_Conditional_1_Template, 3, 0, "button", 0);
        ɵɵconditionalCreate(2, ToastBase_Conditional_2_Template, 3, 5, "div", 1);
        ɵɵconditionalCreate(3, ToastBase_Conditional_3_Template, 2, 1);
        ɵɵconditionalCreate(4, ToastBase_Conditional_4_Template, 2, 2, "div");
      }
      if (rf & 2) {
        const _options_r4 = ɵɵstoreLet(ctx.options());
        ɵɵadvance();
        ɵɵconditional(_options_r4.closeButton ? 1 : -1);
        ɵɵadvance();
        ɵɵconditional(ctx.title() ? 2 : -1);
        ɵɵadvance();
        ɵɵconditional(ctx.message() ? 3 : -1);
        ɵɵadvance();
        ɵɵconditional(_options_r4.progressBar ? 4 : -1);
      }
    },
    encapsulation: 2,
    changeDetection: 0
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(ToastBase, [{
    type: Component,
    args: [{
      selector: "[toast-component]",
      standalone: true,
      changeDetection: ChangeDetectionStrategy.OnPush,
      host: {
        "[class]": "toastClasses()",
        "[style.display]": "displayStyle()",
        "(mouseenter)": "stickAround()",
        "(mouseleave)": "delayedHideToast()",
        "(click)": "tapToast()"
      },
      template: `@let _options = options();

@if (_options.closeButton) {
  <button (click)="remove()" type="button" class="toast-close-button" aria-label="Close">
    <span aria-hidden="true">&times;</span>
  </button>
}

@if (title()) {
  <div [class]="_options.titleClass" [attr.aria-label]="title()">
    {{ title() }}

    @if (duplicatesCount) {
      <ng-container>[{{ duplicatesCount + 1 }}]</ng-container>
    }
  </div>
}

@if (message()) {
  @if (_options.enableHtml) {
    <div role="alert" [class]="_options.messageClass" [innerHTML]="message()"></div>
  } @else {
    <div role="alert" [class]="_options.messageClass" [attr.aria-label]="message()">
      {{ message() }}
    </div>
  }
}

@if (_options.progressBar) {
  <div>
    <div class="toast-progress" [style.width]="width() + '%'"></div>
  </div>
}
`
    }]
  }], () => [], null);
})();
var Toast = class _Toast extends ToastBase {
  params = {
    easeTime: this.toastPackage.config.easeTime,
    easing: "ease-in"
  };
  elementRef = inject(ElementRef);
  remove() {
    if (this.state() === "removed") return;
    clearTimeout(this.timeout);
    this.state.set("removed");
    this.elementRef.nativeElement.classList.add("toast-out");
    this.timeout = this.timeoutsService.setTimeout(() => this.toastrService.remove(this.toastPackage.toastId), +this.params.easeTime);
  }
  static ɵfac = /* @__PURE__ */ (() => {
    let ɵToast_BaseFactory;
    return function Toast_Factory(__ngFactoryType__) {
      return (ɵToast_BaseFactory || (ɵToast_BaseFactory = ɵɵgetInheritedFactory(_Toast)))(__ngFactoryType__ || _Toast);
    };
  })();
  static ɵcmp = ɵɵdefineComponent({
    type: _Toast,
    selectors: [["", "toast-component", ""]],
    hostVars: 4,
    hostBindings: function Toast_HostBindings(rf, ctx) {
      if (rf & 1) {
        ɵɵanimateEnter("toast-in");
      }
      if (rf & 2) {
        ɵɵstyleProp("--animation-easing", ctx.params.easing)("--animation-duration", ctx.params.easeTime + "ms");
      }
    },
    features: [ɵɵInheritDefinitionFeature],
    attrs: _c0,
    decls: 5,
    vars: 5,
    consts: [["type", "button", "aria-label", "Close", 1, "toast-close-button"], [3, "class"], ["type", "button", "aria-label", "Close", 1, "toast-close-button", 3, "click"], ["aria-hidden", "true"], ["role", "alert", 3, "class", "innerHTML"], ["role", "alert", 3, "class"], ["role", "alert", 3, "innerHTML"], ["role", "alert"], [1, "toast-progress"]],
    template: function Toast_Template(rf, ctx) {
      if (rf & 1) {
        ɵɵdeclareLet(0);
        ɵɵconditionalCreate(1, Toast_Conditional_1_Template, 3, 0, "button", 0);
        ɵɵconditionalCreate(2, Toast_Conditional_2_Template, 3, 5, "div", 1);
        ɵɵconditionalCreate(3, Toast_Conditional_3_Template, 2, 1);
        ɵɵconditionalCreate(4, Toast_Conditional_4_Template, 2, 2, "div");
      }
      if (rf & 2) {
        const _options_r4 = ɵɵstoreLet(ctx.options());
        ɵɵadvance();
        ɵɵconditional(_options_r4.closeButton ? 1 : -1);
        ɵɵadvance();
        ɵɵconditional(ctx.title() ? 2 : -1);
        ɵɵadvance();
        ɵɵconditional(ctx.message() ? 3 : -1);
        ɵɵadvance();
        ɵɵconditional(_options_r4.progressBar ? 4 : -1);
      }
    },
    styles: [".toast-in[_nghost-%COMP%]{animation:_ngcontent-%COMP%_toast-animation var(--animation-duration) var(--animation-easing)}.toast-out[_nghost-%COMP%]{animation:_ngcontent-%COMP%_toast-animation var(--animation-duration) var(--animation-easing) reverse forwards}@keyframes _ngcontent-%COMP%_toast-animation{0%{opacity:0}to{opacity:1}}"],
    changeDetection: 0
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(Toast, [{
    type: Component,
    args: [{
      selector: "[toast-component]",
      host: {
        "[style.--animation-easing]": "params.easing",
        "[style.--animation-duration]": 'params.easeTime + "ms"',
        "animate.enter": "toast-in"
      },
      standalone: true,
      changeDetection: ChangeDetectionStrategy.OnPush,
      template: `@let _options = options();

@if (_options.closeButton) {
  <button (click)="remove()" type="button" class="toast-close-button" aria-label="Close">
    <span aria-hidden="true">&times;</span>
  </button>
}

@if (title()) {
  <div [class]="_options.titleClass" [attr.aria-label]="title()">
    {{ title() }}

    @if (duplicatesCount) {
      <ng-container>[{{ duplicatesCount + 1 }}]</ng-container>
    }
  </div>
}

@if (message()) {
  @if (_options.enableHtml) {
    <div role="alert" [class]="_options.messageClass" [innerHTML]="message()"></div>
  } @else {
    <div role="alert" [class]="_options.messageClass" [attr.aria-label]="message()">
      {{ message() }}
    </div>
  }
}

@if (_options.progressBar) {
  <div>
    <div class="toast-progress" [style.width]="width() + '%'"></div>
  </div>
}
`,
      styles: [":host.toast-in{animation:toast-animation var(--animation-duration) var(--animation-easing)}:host.toast-out{animation:toast-animation var(--animation-duration) var(--animation-easing) reverse forwards}@keyframes toast-animation{0%{opacity:0}to{opacity:1}}\n"]
    }]
  }], null, null);
})();
var DefaultGlobalConfig = __spreadProps(__spreadValues({}, DefaultNoComponentGlobalConfig), {
  toastComponent: Toast
});
var provideToastr = (config = {}) => {
  const providers = [{
    provide: TOAST_CONFIG,
    useValue: {
      default: DefaultGlobalConfig,
      config
    }
  }];
  return makeEnvironmentProviders(providers);
};
var ToastrModule = class _ToastrModule {
  static forRoot(config = {}) {
    return {
      ngModule: _ToastrModule,
      providers: [provideToastr(config)]
    };
  }
  static ɵfac = function ToastrModule_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _ToastrModule)();
  };
  static ɵmod = ɵɵdefineNgModule({
    type: _ToastrModule,
    imports: [Toast],
    exports: [Toast]
  });
  static ɵinj = ɵɵdefineInjector({});
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(ToastrModule, [{
    type: NgModule,
    args: [{
      imports: [Toast],
      exports: [Toast]
    }]
  }], null, null);
})();
var ToastrComponentlessModule = class _ToastrComponentlessModule {
  static forRoot(config = {}) {
    return {
      ngModule: ToastrModule,
      providers: [{
        provide: TOAST_CONFIG,
        useValue: {
          default: DefaultNoComponentGlobalConfig,
          config
        }
      }]
    };
  }
  static ɵfac = function ToastrComponentlessModule_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _ToastrComponentlessModule)();
  };
  static ɵmod = ɵɵdefineNgModule({
    type: _ToastrComponentlessModule
  });
  static ɵinj = ɵɵdefineInjector({});
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(ToastrComponentlessModule, [{
    type: NgModule,
    args: [{}]
  }], null, null);
})();
var DefaultNoAnimationsGlobalConfig = __spreadProps(__spreadValues({}, DefaultNoComponentGlobalConfig), {
  toastComponent: ToastBase
});
var ToastNoAnimationModule = class _ToastNoAnimationModule {
  static forRoot(config = {}) {
    return {
      ngModule: _ToastNoAnimationModule,
      providers: [{
        provide: TOAST_CONFIG,
        useValue: {
          default: DefaultNoAnimationsGlobalConfig,
          config
        }
      }]
    };
  }
  static ɵfac = function ToastNoAnimationModule_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _ToastNoAnimationModule)();
  };
  static ɵmod = ɵɵdefineNgModule({
    type: _ToastNoAnimationModule,
    imports: [ToastBase],
    exports: [ToastBase]
  });
  static ɵinj = ɵɵdefineInjector({});
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(ToastNoAnimationModule, [{
    type: NgModule,
    args: [{
      imports: [ToastBase],
      exports: [ToastBase]
    }]
  }], null, null);
})();
export {
  BasePortalHost,
  ComponentPortal,
  DefaultGlobalConfig,
  DefaultNoAnimationsGlobalConfig,
  DefaultNoComponentGlobalConfig,
  Overlay,
  OverlayContainer,
  OverlayRef,
  TOAST_CONFIG,
  Toast,
  ToastContainerDirective,
  ToastBase as ToastNoAnimation,
  ToastNoAnimationModule,
  ToastPackage,
  ToastRef,
  ToastrComponentlessModule,
  ToastrModule,
  ToastrService,
  provideToastr
};
//# sourceMappingURL=ngx-toastr.js.map
