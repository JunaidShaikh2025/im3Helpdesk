import {
  MatIcon,
  MatIconModule
} from "./chunk-UAHV5E7R.js";
import {
  ErrorStateMatcher
} from "./chunk-DALKKVHS.js";
import {
  CdkPortalOutlet,
  PortalModule,
  TemplatePortal
} from "./chunk-HNFXGYRO.js";
import {
  ControlContainer
} from "./chunk-YSZOHGRZ.js";
import {
  MatRippleModule
} from "./chunk-Y4OOKGPF.js";
import {
  MatRipple
} from "./chunk-QCTQPJGZ.js";
import "./chunk-Y6MM5NRT.js";
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
import {
  _getFocusedElementPierceShadowDom
} from "./chunk-XA6252L2.js";
import {
  _animationsDisabled
} from "./chunk-6YA3EALH.js";
import {
  _VisuallyHiddenLoader
} from "./chunk-GKVNUTNV.js";
import "./chunk-ENC6VFIZ.js";
import "./chunk-B4BO3VX5.js";
import {
  _CdkPrivateStyleLoader
} from "./chunk-34LCJAFP.js";
import "./chunk-FDV2MSRL.js";
import "./chunk-42QFQP6S.js";
import "./chunk-N4DOILP3.js";
import "./chunk-GUGIMSVJ.js";
import {
  Platform
} from "./chunk-H4HMK75K.js";
import {
  NgTemplateOutlet
} from "./chunk-YO2PVG56.js";
import "./chunk-NPEN54ZG.js";
import "./chunk-H4ZZ2WUC.js";
import {
  BidiModule,
  Directionality
} from "./chunk-GJERZTUN.js";
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  ContentChild,
  ContentChildren,
  Directive,
  ElementRef,
  EventEmitter,
  Injectable,
  InjectionToken,
  Input,
  NgModule,
  NgZone,
  Output,
  QueryList,
  Renderer2,
  TemplateRef,
  ViewChild,
  ViewChildren,
  ViewContainerRef,
  ViewEncapsulation,
  booleanAttribute,
  computed,
  inject,
  input,
  numberAttribute,
  setClassMetadata,
  signal,
  ɵɵInheritDefinitionFeature,
  ɵɵNgOnChangesFeature,
  ɵɵProvidersFeature,
  ɵɵadvance,
  ɵɵattribute,
  ɵɵclassMap,
  ɵɵclassProp,
  ɵɵconditional,
  ɵɵconditionalCreate,
  ɵɵcontentQuery,
  ɵɵdefineComponent,
  ɵɵdefineDirective,
  ɵɵdefineInjectable,
  ɵɵdefineInjector,
  ɵɵdefineNgModule,
  ɵɵdomProperty,
  ɵɵdomTemplate,
  ɵɵelement,
  ɵɵelementContainer,
  ɵɵelementEnd,
  ɵɵelementStart,
  ɵɵgetCurrentView,
  ɵɵgetInheritedFactory,
  ɵɵinterpolate1,
  ɵɵlistener,
  ɵɵloadQuery,
  ɵɵnextContext,
  ɵɵprojection,
  ɵɵprojectionDef,
  ɵɵproperty,
  ɵɵpureFunction1,
  ɵɵpureFunction3,
  ɵɵqueryRefresh,
  ɵɵreference,
  ɵɵrepeater,
  ɵɵrepeaterCreate,
  ɵɵrepeaterTrackByIdentity,
  ɵɵresetView,
  ɵɵrestoreView,
  ɵɵstyleProp,
  ɵɵtemplate,
  ɵɵtemplateRefExtractor,
  ɵɵtext,
  ɵɵtextInterpolate,
  ɵɵviewQuery
} from "./chunk-3BW4HTRP.js";
import "./chunk-HWYXSU2G.js";
import "./chunk-JRFR6BLO.js";
import {
  Subject,
  Subscription,
  map,
  of,
  startWith,
  switchMap,
  takeUntil
} from "./chunk-MARUHEWW.js";
import "./chunk-XSXJULYR.js";

// node_modules/@angular/cdk/fesm2022/stepper.mjs
var _c0 = ["*"];
function CdkStep_ng_template_0_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵprojection(0);
  }
}
var CdkStepHeader = class _CdkStepHeader {
  _elementRef = inject(ElementRef);
  constructor() {
  }
  focus() {
    this._elementRef.nativeElement.focus();
  }
  static ɵfac = function CdkStepHeader_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _CdkStepHeader)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _CdkStepHeader,
    selectors: [["", "cdkStepHeader", ""]],
    hostAttrs: ["role", "tab"]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(CdkStepHeader, [{
    type: Directive,
    args: [{
      selector: "[cdkStepHeader]",
      host: {
        "role": "tab"
      }
    }]
  }], () => [], null);
})();
var CdkStepLabel = class _CdkStepLabel {
  template = inject(TemplateRef);
  constructor() {
  }
  static ɵfac = function CdkStepLabel_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _CdkStepLabel)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _CdkStepLabel,
    selectors: [["", "cdkStepLabel", ""]]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(CdkStepLabel, [{
    type: Directive,
    args: [{
      selector: "[cdkStepLabel]"
    }]
  }], () => [], null);
})();
var STEP_STATE = {
  NUMBER: "number",
  EDIT: "edit",
  DONE: "done",
  ERROR: "error"
};
var STEPPER_GLOBAL_OPTIONS = new InjectionToken("STEPPER_GLOBAL_OPTIONS");
var CdkStep = class _CdkStep {
  _stepperOptions;
  _stepper = inject(CdkStepper);
  _displayDefaultIndicatorType;
  stepLabel;
  _childForms;
  content;
  stepControl;
  get interacted() {
    return this._interacted();
  }
  set interacted(value) {
    this._interacted.set(value);
  }
  _interacted = signal(false, ...ngDevMode ? [{
    debugName: "_interacted"
  }] : []);
  interactedStream = new EventEmitter();
  label;
  errorMessage;
  ariaLabel;
  ariaLabelledby;
  get state() {
    return this._state();
  }
  set state(value) {
    this._state.set(value);
  }
  _state = signal(void 0, ...ngDevMode ? [{
    debugName: "_state"
  }] : []);
  get editable() {
    return this._editable();
  }
  set editable(value) {
    this._editable.set(value);
  }
  _editable = signal(true, ...ngDevMode ? [{
    debugName: "_editable"
  }] : []);
  optional = false;
  get completed() {
    const override = this._completedOverride();
    const interacted = this._interacted();
    if (override != null) {
      return override;
    }
    return interacted && (!this.stepControl || this.stepControl.valid);
  }
  set completed(value) {
    this._completedOverride.set(value);
  }
  _completedOverride = signal(null, ...ngDevMode ? [{
    debugName: "_completedOverride"
  }] : []);
  index = signal(-1, ...ngDevMode ? [{
    debugName: "index"
  }] : []);
  isSelected = computed(() => this._stepper.selectedIndex === this.index(), ...ngDevMode ? [{
    debugName: "isSelected"
  }] : []);
  indicatorType = computed(() => {
    const selected = this.isSelected();
    const completed = this.completed;
    const defaultState = this._state() ?? STEP_STATE.NUMBER;
    const editable = this._editable();
    if (this._showError() && this.hasError && !selected) {
      return STEP_STATE.ERROR;
    }
    if (this._displayDefaultIndicatorType) {
      if (!completed || selected) {
        return STEP_STATE.NUMBER;
      }
      return editable ? STEP_STATE.EDIT : STEP_STATE.DONE;
    } else {
      if (completed && !selected) {
        return STEP_STATE.DONE;
      } else if (completed && selected) {
        return defaultState;
      }
      return editable && selected ? STEP_STATE.EDIT : defaultState;
    }
  }, ...ngDevMode ? [{
    debugName: "indicatorType"
  }] : []);
  isNavigable = computed(() => {
    const isSelected = this.isSelected();
    const isCompleted = this.completed;
    return isCompleted || isSelected || !this._stepper.linear;
  }, ...ngDevMode ? [{
    debugName: "isNavigable"
  }] : []);
  get hasError() {
    const customError = this._customError();
    return customError == null ? this._getDefaultError() : customError;
  }
  set hasError(value) {
    this._customError.set(value);
  }
  _customError = signal(null, ...ngDevMode ? [{
    debugName: "_customError"
  }] : []);
  _getDefaultError() {
    return this.interacted && !!this.stepControl?.invalid;
  }
  constructor() {
    const stepperOptions = inject(STEPPER_GLOBAL_OPTIONS, {
      optional: true
    });
    this._stepperOptions = stepperOptions ? stepperOptions : {};
    this._displayDefaultIndicatorType = this._stepperOptions.displayDefaultIndicatorType !== false;
  }
  select() {
    this._stepper.selected = this;
  }
  reset() {
    this._interacted.set(false);
    if (this._completedOverride() != null) {
      this._completedOverride.set(false);
    }
    if (this._customError() != null) {
      this._customError.set(false);
    }
    if (this.stepControl) {
      this._childForms?.forEach((form) => form.resetForm?.());
      this.stepControl.reset();
    }
  }
  ngOnChanges() {
    this._stepper._stateChanged();
  }
  _markAsInteracted() {
    if (!this._interacted()) {
      this._interacted.set(true);
      this.interactedStream.emit(this);
    }
  }
  _showError() {
    return this._stepperOptions.showError ?? this._customError() != null;
  }
  static ɵfac = function CdkStep_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _CdkStep)();
  };
  static ɵcmp = ɵɵdefineComponent({
    type: _CdkStep,
    selectors: [["cdk-step"]],
    contentQueries: function CdkStep_ContentQueries(rf, ctx, dirIndex) {
      if (rf & 1) {
        ɵɵcontentQuery(dirIndex, CdkStepLabel, 5)(dirIndex, ControlContainer, 5);
      }
      if (rf & 2) {
        let _t;
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx.stepLabel = _t.first);
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._childForms = _t);
      }
    },
    viewQuery: function CdkStep_Query(rf, ctx) {
      if (rf & 1) {
        ɵɵviewQuery(TemplateRef, 7);
      }
      if (rf & 2) {
        let _t;
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx.content = _t.first);
      }
    },
    inputs: {
      stepControl: "stepControl",
      label: "label",
      errorMessage: "errorMessage",
      ariaLabel: [0, "aria-label", "ariaLabel"],
      ariaLabelledby: [0, "aria-labelledby", "ariaLabelledby"],
      state: "state",
      editable: [2, "editable", "editable", booleanAttribute],
      optional: [2, "optional", "optional", booleanAttribute],
      completed: [2, "completed", "completed", booleanAttribute],
      hasError: [2, "hasError", "hasError", booleanAttribute]
    },
    outputs: {
      interactedStream: "interacted"
    },
    exportAs: ["cdkStep"],
    features: [ɵɵNgOnChangesFeature],
    ngContentSelectors: _c0,
    decls: 1,
    vars: 0,
    template: function CdkStep_Template(rf, ctx) {
      if (rf & 1) {
        ɵɵprojectionDef();
        ɵɵdomTemplate(0, CdkStep_ng_template_0_Template, 1, 0, "ng-template");
      }
    },
    encapsulation: 2,
    changeDetection: 0
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(CdkStep, [{
    type: Component,
    args: [{
      selector: "cdk-step",
      exportAs: "cdkStep",
      template: "<ng-template><ng-content/></ng-template>",
      encapsulation: ViewEncapsulation.None,
      changeDetection: ChangeDetectionStrategy.OnPush
    }]
  }], () => [], {
    stepLabel: [{
      type: ContentChild,
      args: [CdkStepLabel]
    }],
    _childForms: [{
      type: ContentChildren,
      args: [ControlContainer, {
        descendants: true
      }]
    }],
    content: [{
      type: ViewChild,
      args: [TemplateRef, {
        static: true
      }]
    }],
    stepControl: [{
      type: Input
    }],
    interactedStream: [{
      type: Output,
      args: ["interacted"]
    }],
    label: [{
      type: Input
    }],
    errorMessage: [{
      type: Input
    }],
    ariaLabel: [{
      type: Input,
      args: ["aria-label"]
    }],
    ariaLabelledby: [{
      type: Input,
      args: ["aria-labelledby"]
    }],
    state: [{
      type: Input
    }],
    editable: [{
      type: Input,
      args: [{
        transform: booleanAttribute
      }]
    }],
    optional: [{
      type: Input,
      args: [{
        transform: booleanAttribute
      }]
    }],
    completed: [{
      type: Input,
      args: [{
        transform: booleanAttribute
      }]
    }],
    hasError: [{
      type: Input,
      args: [{
        transform: booleanAttribute
      }]
    }]
  });
})();
var CdkStepper = class _CdkStepper {
  _dir = inject(Directionality, {
    optional: true
  });
  _changeDetectorRef = inject(ChangeDetectorRef);
  _elementRef = inject(ElementRef);
  _destroyed = new Subject();
  _keyManager;
  _steps;
  steps = new QueryList();
  _stepHeader;
  _sortedHeaders = new QueryList();
  get linear() {
    return this._linear();
  }
  set linear(value) {
    this._linear.set(value);
  }
  _linear = signal(false, ...ngDevMode ? [{
    debugName: "_linear"
  }] : []);
  get selectedIndex() {
    return this._selectedIndex();
  }
  set selectedIndex(index) {
    if (this._steps) {
      if (!this._isValidIndex(index) && (typeof ngDevMode === "undefined" || ngDevMode)) {
        throw Error("cdkStepper: Cannot assign out-of-bounds value to `selectedIndex`.");
      }
      if (this.selectedIndex !== index) {
        this.selected?._markAsInteracted();
        if (!this._anyControlsInvalidOrPending(index) && (index >= this.selectedIndex || this.steps.toArray()[index].editable)) {
          this._updateSelectedItemIndex(index);
        }
      }
    } else {
      this._selectedIndex.set(index);
    }
  }
  _selectedIndex = signal(0, ...ngDevMode ? [{
    debugName: "_selectedIndex"
  }] : []);
  get selected() {
    return this.steps ? this.steps.toArray()[this.selectedIndex] : void 0;
  }
  set selected(step) {
    this.selectedIndex = step && this.steps ? this.steps.toArray().indexOf(step) : -1;
  }
  selectionChange = new EventEmitter();
  selectedIndexChange = new EventEmitter();
  _groupId = inject(_IdGenerator).getId("cdk-stepper-");
  get orientation() {
    return this._orientation;
  }
  set orientation(value) {
    this._orientation = value;
    if (this._keyManager) {
      this._keyManager.withVerticalOrientation(value === "vertical");
    }
  }
  _orientation = "horizontal";
  constructor() {
  }
  ngAfterContentInit() {
    this._steps.changes.pipe(startWith(this._steps), takeUntil(this._destroyed)).subscribe((steps) => {
      this.steps.reset(steps.filter((step) => step._stepper === this));
      this.steps.forEach((step, index) => step.index.set(index));
      this.steps.notifyOnChanges();
    });
  }
  ngAfterViewInit() {
    this._stepHeader.changes.pipe(startWith(this._stepHeader), takeUntil(this._destroyed)).subscribe((headers) => {
      this._sortedHeaders.reset(headers.toArray().sort((a, b) => {
        const documentPosition = a._elementRef.nativeElement.compareDocumentPosition(b._elementRef.nativeElement);
        return documentPosition & Node.DOCUMENT_POSITION_FOLLOWING ? -1 : 1;
      }));
      this._sortedHeaders.notifyOnChanges();
    });
    this._keyManager = new FocusKeyManager(this._sortedHeaders).withWrap().withHomeAndEnd().withVerticalOrientation(this._orientation === "vertical");
    this._keyManager.updateActiveItem(this.selectedIndex);
    (this._dir ? this._dir.change : of()).pipe(startWith(this._layoutDirection()), takeUntil(this._destroyed)).subscribe((direction) => this._keyManager?.withHorizontalOrientation(direction));
    this._keyManager.updateActiveItem(this.selectedIndex);
    this.steps.changes.subscribe(() => {
      if (!this.selected) {
        this._selectedIndex.set(Math.max(this.selectedIndex - 1, 0));
      }
    });
    if (!this._isValidIndex(this.selectedIndex)) {
      this._selectedIndex.set(0);
    }
    if (this.linear && this.selectedIndex > 0) {
      const visitedSteps = this.steps.toArray().slice(0, this._selectedIndex());
      for (const step of visitedSteps) {
        step._markAsInteracted();
      }
    }
  }
  ngOnDestroy() {
    this._keyManager?.destroy();
    this.steps.destroy();
    this._sortedHeaders.destroy();
    this._destroyed.next();
    this._destroyed.complete();
  }
  next() {
    this.selectedIndex = Math.min(this._selectedIndex() + 1, this.steps.length - 1);
  }
  previous() {
    this.selectedIndex = Math.max(this._selectedIndex() - 1, 0);
  }
  reset() {
    this._updateSelectedItemIndex(0);
    this.steps.forEach((step) => step.reset());
    this._stateChanged();
  }
  _getStepLabelId(i) {
    return `${this._groupId}-label-${i}`;
  }
  _getStepContentId(i) {
    return `${this._groupId}-content-${i}`;
  }
  _stateChanged() {
    this._changeDetectorRef.markForCheck();
  }
  _getAnimationDirection(index) {
    const position = index - this._selectedIndex();
    if (position < 0) {
      return this._layoutDirection() === "rtl" ? "next" : "previous";
    } else if (position > 0) {
      return this._layoutDirection() === "rtl" ? "previous" : "next";
    }
    return "current";
  }
  _getFocusIndex() {
    return this._keyManager ? this._keyManager.activeItemIndex : this._selectedIndex();
  }
  _updateSelectedItemIndex(newIndex) {
    const stepsArray = this.steps.toArray();
    const selectedIndex = this._selectedIndex();
    this.selectionChange.emit({
      selectedIndex: newIndex,
      previouslySelectedIndex: selectedIndex,
      selectedStep: stepsArray[newIndex],
      previouslySelectedStep: stepsArray[selectedIndex]
    });
    if (this._keyManager) {
      this._containsFocus() ? this._keyManager.setActiveItem(newIndex) : this._keyManager.updateActiveItem(newIndex);
    }
    this._selectedIndex.set(newIndex);
    this.selectedIndexChange.emit(newIndex);
    this._stateChanged();
  }
  _onKeydown(event) {
    const hasModifier = hasModifierKey(event);
    const keyCode = event.keyCode;
    const manager = this._keyManager;
    if (manager?.activeItemIndex != null && !hasModifier && (keyCode === SPACE || keyCode === ENTER)) {
      this.selectedIndex = manager.activeItemIndex;
      event.preventDefault();
    } else {
      manager?.setFocusOrigin("keyboard").onKeydown(event);
    }
  }
  _anyControlsInvalidOrPending(index) {
    if (this.linear && index >= 0) {
      return this.steps.toArray().slice(0, index).some((step) => {
        const control = step.stepControl;
        const isIncomplete = control ? control.invalid || control.pending || !step.interacted : !step.completed;
        return isIncomplete && !step.optional && !step._completedOverride();
      });
    }
    return false;
  }
  _layoutDirection() {
    return this._dir && this._dir.value === "rtl" ? "rtl" : "ltr";
  }
  _containsFocus() {
    const stepperElement = this._elementRef.nativeElement;
    const focusedElement = _getFocusedElementPierceShadowDom();
    return stepperElement === focusedElement || stepperElement.contains(focusedElement);
  }
  _isValidIndex(index) {
    return index > -1 && (!this.steps || index < this.steps.length);
  }
  static ɵfac = function CdkStepper_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _CdkStepper)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _CdkStepper,
    selectors: [["", "cdkStepper", ""]],
    contentQueries: function CdkStepper_ContentQueries(rf, ctx, dirIndex) {
      if (rf & 1) {
        ɵɵcontentQuery(dirIndex, CdkStep, 5)(dirIndex, CdkStepHeader, 5);
      }
      if (rf & 2) {
        let _t;
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._steps = _t);
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._stepHeader = _t);
      }
    },
    inputs: {
      linear: [2, "linear", "linear", booleanAttribute],
      selectedIndex: [2, "selectedIndex", "selectedIndex", numberAttribute],
      selected: "selected",
      orientation: "orientation"
    },
    outputs: {
      selectionChange: "selectionChange",
      selectedIndexChange: "selectedIndexChange"
    },
    exportAs: ["cdkStepper"]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(CdkStepper, [{
    type: Directive,
    args: [{
      selector: "[cdkStepper]",
      exportAs: "cdkStepper"
    }]
  }], () => [], {
    _steps: [{
      type: ContentChildren,
      args: [CdkStep, {
        descendants: true
      }]
    }],
    _stepHeader: [{
      type: ContentChildren,
      args: [CdkStepHeader, {
        descendants: true
      }]
    }],
    linear: [{
      type: Input,
      args: [{
        transform: booleanAttribute
      }]
    }],
    selectedIndex: [{
      type: Input,
      args: [{
        transform: numberAttribute
      }]
    }],
    selected: [{
      type: Input
    }],
    selectionChange: [{
      type: Output
    }],
    selectedIndexChange: [{
      type: Output
    }],
    orientation: [{
      type: Input
    }]
  });
})();
var CdkStepperNext = class _CdkStepperNext {
  _stepper = inject(CdkStepper);
  type = "submit";
  constructor() {
  }
  static ɵfac = function CdkStepperNext_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _CdkStepperNext)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _CdkStepperNext,
    selectors: [["button", "cdkStepperNext", ""]],
    hostVars: 1,
    hostBindings: function CdkStepperNext_HostBindings(rf, ctx) {
      if (rf & 1) {
        ɵɵlistener("click", function CdkStepperNext_click_HostBindingHandler() {
          return ctx._stepper.next();
        });
      }
      if (rf & 2) {
        ɵɵdomProperty("type", ctx.type);
      }
    },
    inputs: {
      type: "type"
    }
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(CdkStepperNext, [{
    type: Directive,
    args: [{
      selector: "button[cdkStepperNext]",
      host: {
        "[type]": "type",
        "(click)": "_stepper.next()"
      }
    }]
  }], () => [], {
    type: [{
      type: Input
    }]
  });
})();
var CdkStepperPrevious = class _CdkStepperPrevious {
  _stepper = inject(CdkStepper);
  type = "button";
  constructor() {
  }
  static ɵfac = function CdkStepperPrevious_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _CdkStepperPrevious)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _CdkStepperPrevious,
    selectors: [["button", "cdkStepperPrevious", ""]],
    hostVars: 1,
    hostBindings: function CdkStepperPrevious_HostBindings(rf, ctx) {
      if (rf & 1) {
        ɵɵlistener("click", function CdkStepperPrevious_click_HostBindingHandler() {
          return ctx._stepper.previous();
        });
      }
      if (rf & 2) {
        ɵɵdomProperty("type", ctx.type);
      }
    },
    inputs: {
      type: "type"
    }
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(CdkStepperPrevious, [{
    type: Directive,
    args: [{
      selector: "button[cdkStepperPrevious]",
      host: {
        "[type]": "type",
        "(click)": "_stepper.previous()"
      }
    }]
  }], () => [], {
    type: [{
      type: Input
    }]
  });
})();
var CdkStepperModule = class _CdkStepperModule {
  static ɵfac = function CdkStepperModule_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _CdkStepperModule)();
  };
  static ɵmod = ɵɵdefineNgModule({
    type: _CdkStepperModule,
    imports: [BidiModule, CdkStep, CdkStepper, CdkStepHeader, CdkStepLabel, CdkStepperNext, CdkStepperPrevious],
    exports: [CdkStep, CdkStepper, CdkStepHeader, CdkStepLabel, CdkStepperNext, CdkStepperPrevious]
  });
  static ɵinj = ɵɵdefineInjector({
    imports: [BidiModule]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(CdkStepperModule, [{
    type: NgModule,
    args: [{
      imports: [BidiModule, CdkStep, CdkStepper, CdkStepHeader, CdkStepLabel, CdkStepperNext, CdkStepperPrevious],
      exports: [CdkStep, CdkStepper, CdkStepHeader, CdkStepLabel, CdkStepperNext, CdkStepperPrevious]
    }]
  }], null, null);
})();

// node_modules/@angular/material/fesm2022/stepper.mjs
var _c02 = (a0, a1, a2) => ({
  index: a0,
  active: a1,
  optional: a2
});
function MatStepHeader_Conditional_3_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementContainer(0, 2);
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext();
    ɵɵproperty("ngTemplateOutlet", ctx_r0.iconOverrides[ctx_r0.state])("ngTemplateOutletContext", ɵɵpureFunction3(2, _c02, ctx_r0.index, ctx_r0.active, ctx_r0.optional));
  }
}
function MatStepHeader_Conditional_4_Case_0_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "span", 7);
    ɵɵtext(1);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext(2);
    ɵɵadvance();
    ɵɵtextInterpolate(ctx_r0._getDefaultTextForState(ctx_r0.state));
  }
}
function MatStepHeader_Conditional_4_Case_1_Conditional_0_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "span", 8);
    ɵɵtext(1);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext(3);
    ɵɵadvance();
    ɵɵtextInterpolate(ctx_r0._intl.completedLabel);
  }
}
function MatStepHeader_Conditional_4_Case_1_Conditional_1_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "span", 8);
    ɵɵtext(1);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext(3);
    ɵɵadvance();
    ɵɵtextInterpolate(ctx_r0._intl.editableLabel);
  }
}
function MatStepHeader_Conditional_4_Case_1_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵconditionalCreate(0, MatStepHeader_Conditional_4_Case_1_Conditional_0_Template, 2, 1, "span", 8)(1, MatStepHeader_Conditional_4_Case_1_Conditional_1_Template, 2, 1, "span", 8);
    ɵɵelementStart(2, "mat-icon", 7);
    ɵɵtext(3);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext(2);
    ɵɵconditional(ctx_r0.state === "done" ? 0 : ctx_r0.state === "edit" ? 1 : -1);
    ɵɵadvance(3);
    ɵɵtextInterpolate(ctx_r0._getDefaultTextForState(ctx_r0.state));
  }
}
function MatStepHeader_Conditional_4_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵconditionalCreate(0, MatStepHeader_Conditional_4_Case_0_Template, 2, 1, "span", 7)(1, MatStepHeader_Conditional_4_Case_1_Template, 4, 2);
  }
  if (rf & 2) {
    let tmp_1_0;
    const ctx_r0 = ɵɵnextContext();
    ɵɵconditional((tmp_1_0 = ctx_r0.state) === "number" ? 0 : 1);
  }
}
function MatStepHeader_Conditional_6_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "div", 4);
    ɵɵelementContainer(1, 9);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    ɵɵadvance();
    ɵɵproperty("ngTemplateOutlet", ctx.template);
  }
}
function MatStepHeader_Conditional_7_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "div", 4);
    ɵɵtext(1);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext();
    ɵɵadvance();
    ɵɵtextInterpolate(ctx_r0.label);
  }
}
function MatStepHeader_Conditional_8_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "div", 5);
    ɵɵtext(1);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext();
    ɵɵadvance();
    ɵɵtextInterpolate(ctx_r0._intl.optionalLabel);
  }
}
function MatStepHeader_Conditional_9_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "div", 6);
    ɵɵtext(1);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext();
    ɵɵadvance();
    ɵɵtextInterpolate(ctx_r0.errorMessage);
  }
}
var _c1 = ["*"];
function MatStep_ng_template_0_ng_template_1_Template(rf, ctx) {
}
function MatStep_ng_template_0_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵprojection(0);
    ɵɵtemplate(1, MatStep_ng_template_0_ng_template_1_Template, 0, 0, "ng-template", 0);
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext();
    ɵɵadvance();
    ɵɵproperty("cdkPortalOutlet", ctx_r0._portal);
  }
}
var _c2 = ["animatedContainer"];
var _c3 = (a0) => ({
  steps: a0
});
var _c4 = (a0) => ({
  step: a0
});
function MatStepper_Conditional_0_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵprojection(0);
  }
}
function MatStepper_Case_1_Conditional_1_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "div", 5);
    ɵɵelementContainer(1, 9)(2, 6);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext(2);
    const horizontalStepsTemplate_r2 = ɵɵreference(6);
    ɵɵadvance();
    ɵɵproperty("ngTemplateOutlet", ctx_r0.headerPrefix());
    ɵɵadvance();
    ɵɵproperty("ngTemplateOutlet", horizontalStepsTemplate_r2)("ngTemplateOutletContext", ɵɵpureFunction1(3, _c3, ctx_r0.steps));
  }
}
function MatStepper_Case_1_Conditional_2_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementContainer(0, 6);
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext(2);
    const horizontalStepsTemplate_r2 = ɵɵreference(6);
    ɵɵproperty("ngTemplateOutlet", horizontalStepsTemplate_r2)("ngTemplateOutletContext", ɵɵpureFunction1(2, _c3, ctx_r0.steps));
  }
}
function MatStepper_Case_1_For_5_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "div", 10, 2);
    ɵɵelementContainer(2, 9);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const step_r3 = ctx.$implicit;
    const $index_r4 = ctx.$index;
    const ctx_r0 = ɵɵnextContext(2);
    ɵɵclassMap("mat-horizontal-stepper-content-" + ctx_r0._getAnimationDirection($index_r4));
    ɵɵproperty("id", ctx_r0._getStepContentId($index_r4));
    ɵɵattribute("aria-labelledby", ctx_r0._getStepLabelId($index_r4))("inert", ctx_r0.selectedIndex === $index_r4 ? null : "");
    ɵɵadvance(2);
    ɵɵproperty("ngTemplateOutlet", step_r3.content);
  }
}
function MatStepper_Case_1_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "div", 3);
    ɵɵconditionalCreate(1, MatStepper_Case_1_Conditional_1_Template, 3, 5, "div", 5)(2, MatStepper_Case_1_Conditional_2_Template, 1, 4, "ng-container", 6);
    ɵɵelementStart(3, "div", 7);
    ɵɵrepeaterCreate(4, MatStepper_Case_1_For_5_Template, 3, 6, "div", 8, ɵɵrepeaterTrackByIdentity);
    ɵɵelementEnd()();
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext();
    ɵɵadvance();
    ɵɵconditional(ctx_r0.headerPrefix() ? 1 : 2);
    ɵɵadvance(3);
    ɵɵrepeater(ctx_r0.steps);
  }
}
function MatStepper_Case_2_Conditional_1_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementContainer(0, 9);
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext(2);
    ɵɵproperty("ngTemplateOutlet", ctx_r0.headerPrefix());
  }
}
function MatStepper_Case_2_For_3_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "div", 11);
    ɵɵelementContainer(1, 6);
    ɵɵelementStart(2, "div", 12, 2)(4, "div", 13)(5, "div", 14);
    ɵɵelementContainer(6, 9);
    ɵɵelementEnd()()()();
  }
  if (rf & 2) {
    const step_r5 = ctx.$implicit;
    const $index_r6 = ctx.$index;
    const ɵ$index_29_r7 = ctx.$index;
    const ɵ$count_29_r8 = ctx.$count;
    const ctx_r0 = ɵɵnextContext(2);
    const stepTemplate_r9 = ɵɵreference(4);
    ɵɵadvance();
    ɵɵproperty("ngTemplateOutlet", stepTemplate_r9)("ngTemplateOutletContext", ɵɵpureFunction1(10, _c4, step_r5));
    ɵɵadvance();
    ɵɵclassProp("mat-stepper-vertical-line", !(ɵ$index_29_r7 === ɵ$count_29_r8 - 1))("mat-vertical-content-container-active", ctx_r0.selectedIndex === $index_r6);
    ɵɵattribute("inert", ctx_r0.selectedIndex === $index_r6 ? null : "");
    ɵɵadvance(2);
    ɵɵproperty("id", ctx_r0._getStepContentId($index_r6));
    ɵɵattribute("aria-labelledby", ctx_r0._getStepLabelId($index_r6));
    ɵɵadvance(2);
    ɵɵproperty("ngTemplateOutlet", step_r5.content);
  }
}
function MatStepper_Case_2_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "div", 4);
    ɵɵconditionalCreate(1, MatStepper_Case_2_Conditional_1_Template, 1, 1, "ng-container", 9);
    ɵɵrepeaterCreate(2, MatStepper_Case_2_For_3_Template, 7, 12, "div", 11, ɵɵrepeaterTrackByIdentity);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const ctx_r0 = ɵɵnextContext();
    ɵɵadvance();
    ɵɵconditional(ctx_r0.headerPrefix() ? 1 : -1);
    ɵɵadvance();
    ɵɵrepeater(ctx_r0.steps);
  }
}
function MatStepper_ng_template_3_Template(rf, ctx) {
  if (rf & 1) {
    const _r10 = ɵɵgetCurrentView();
    ɵɵelementStart(0, "mat-step-header", 15);
    ɵɵlistener("click", function MatStepper_ng_template_3_Template_mat_step_header_click_0_listener() {
      const step_r11 = ɵɵrestoreView(_r10).step;
      return ɵɵresetView(step_r11.select());
    })("keydown", function MatStepper_ng_template_3_Template_mat_step_header_keydown_0_listener($event) {
      ɵɵrestoreView(_r10);
      const ctx_r0 = ɵɵnextContext();
      return ɵɵresetView(ctx_r0._onKeydown($event));
    });
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const step_r11 = ctx.step;
    const ctx_r0 = ɵɵnextContext();
    ɵɵclassProp("mat-horizontal-stepper-header", ctx_r0.orientation === "horizontal")("mat-vertical-stepper-header", ctx_r0.orientation === "vertical");
    ɵɵproperty("tabIndex", ctx_r0._getFocusIndex() === step_r11.index() ? 0 : -1)("id", ctx_r0._getStepLabelId(step_r11.index()))("index", step_r11.index())("state", step_r11.indicatorType())("label", step_r11.stepLabel || step_r11.label)("selected", step_r11.isSelected())("active", step_r11.isNavigable())("optional", step_r11.optional)("errorMessage", step_r11.errorMessage)("iconOverrides", ctx_r0._iconOverrides)("disableRipple", ctx_r0.disableRipple || !step_r11.isNavigable())("color", step_r11.color || ctx_r0.color);
    ɵɵattribute("role", ctx_r0.orientation === "horizontal" ? "tab" : "button")("aria-posinset", ctx_r0.orientation === "horizontal" ? step_r11.index() + 1 : null)("aria-setsize", ctx_r0.orientation === "horizontal" ? ctx_r0.steps.length : null)("aria-selected", ctx_r0.orientation === "horizontal" ? step_r11.isSelected() : null)("aria-current", ctx_r0.orientation === "vertical" && step_r11.isSelected() ? "step" : null)("aria-disabled", ctx_r0.orientation === "vertical" && step_r11.isSelected() ? "true" : null)("aria-expanded", ctx_r0.orientation === "vertical" ? step_r11.isSelected() : null)("aria-controls", ctx_r0._getStepContentId(step_r11.index()))("aria-label", step_r11.ariaLabel || null)("aria-labelledby", !step_r11.ariaLabel && step_r11.ariaLabelledby ? step_r11.ariaLabelledby : null)("aria-disabled", step_r11.isNavigable() ? null : true);
  }
}
function MatStepper_ng_template_5_For_2_Conditional_1_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelement(0, "div", 17);
  }
}
function MatStepper_ng_template_5_For_2_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementContainer(0, 6);
    ɵɵconditionalCreate(1, MatStepper_ng_template_5_For_2_Conditional_1_Template, 1, 0, "div", 17);
  }
  if (rf & 2) {
    const step_r12 = ctx.$implicit;
    const ɵ$index_50_r13 = ctx.$index;
    const ɵ$count_50_r14 = ctx.$count;
    ɵɵnextContext(2);
    const stepTemplate_r9 = ɵɵreference(4);
    ɵɵproperty("ngTemplateOutlet", stepTemplate_r9)("ngTemplateOutletContext", ɵɵpureFunction1(3, _c4, step_r12));
    ɵɵadvance();
    ɵɵconditional(!(ɵ$index_50_r13 === ɵ$count_50_r14 - 1) ? 1 : -1);
  }
}
function MatStepper_ng_template_5_Template(rf, ctx) {
  if (rf & 1) {
    ɵɵelementStart(0, "div", 16);
    ɵɵrepeaterCreate(1, MatStepper_ng_template_5_For_2_Template, 2, 5, null, null, ɵɵrepeaterTrackByIdentity);
    ɵɵelementEnd();
  }
  if (rf & 2) {
    const steps_r15 = ctx.steps;
    ɵɵadvance();
    ɵɵrepeater(steps_r15);
  }
}
var MatStepLabel = class _MatStepLabel extends CdkStepLabel {
  static ɵfac = /* @__PURE__ */ (() => {
    let ɵMatStepLabel_BaseFactory;
    return function MatStepLabel_Factory(__ngFactoryType__) {
      return (ɵMatStepLabel_BaseFactory || (ɵMatStepLabel_BaseFactory = ɵɵgetInheritedFactory(_MatStepLabel)))(__ngFactoryType__ || _MatStepLabel);
    };
  })();
  static ɵdir = ɵɵdefineDirective({
    type: _MatStepLabel,
    selectors: [["", "matStepLabel", ""]],
    features: [ɵɵInheritDefinitionFeature]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatStepLabel, [{
    type: Directive,
    args: [{
      selector: "[matStepLabel]"
    }]
  }], null, null);
})();
var MatStepperIntl = class _MatStepperIntl {
  changes = new Subject();
  optionalLabel = "Optional";
  completedLabel = "Completed";
  editableLabel = "Editable";
  static ɵfac = function MatStepperIntl_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatStepperIntl)();
  };
  static ɵprov = ɵɵdefineInjectable({
    token: _MatStepperIntl,
    factory: _MatStepperIntl.ɵfac,
    providedIn: "root"
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatStepperIntl, [{
    type: Injectable,
    args: [{
      providedIn: "root"
    }]
  }], null, null);
})();
var MatStepHeader = class _MatStepHeader extends CdkStepHeader {
  _intl = inject(MatStepperIntl);
  _focusMonitor = inject(FocusMonitor);
  _intlSubscription;
  state;
  label;
  errorMessage;
  iconOverrides;
  index;
  selected = false;
  active = false;
  optional = false;
  disableRipple = false;
  color;
  constructor() {
    super();
    const styleLoader = inject(_CdkPrivateStyleLoader);
    styleLoader.load(_StructuralStylesLoader);
    styleLoader.load(_VisuallyHiddenLoader);
    const changeDetectorRef = inject(ChangeDetectorRef);
    this._intlSubscription = this._intl.changes.subscribe(() => changeDetectorRef.markForCheck());
  }
  ngAfterViewInit() {
    this._focusMonitor.monitor(this._elementRef, true);
  }
  ngOnDestroy() {
    this._intlSubscription.unsubscribe();
    this._focusMonitor.stopMonitoring(this._elementRef);
  }
  focus(origin, options) {
    if (origin) {
      this._focusMonitor.focusVia(this._elementRef, origin, options);
    } else {
      this._elementRef.nativeElement.focus(options);
    }
  }
  _stringLabel() {
    return this.label instanceof MatStepLabel ? null : this.label;
  }
  _templateLabel() {
    return this.label instanceof MatStepLabel ? this.label : null;
  }
  _getHostElement() {
    return this._elementRef.nativeElement;
  }
  _getDefaultTextForState(state) {
    if (state == "number") {
      return `${this.index + 1}`;
    }
    if (state == "edit") {
      return "create";
    }
    if (state == "error") {
      return "warning";
    }
    return state;
  }
  _hasEmptyLabel() {
    return !this._stringLabel() && !this._templateLabel() && !this._hasOptionalLabel() && !this._hasErrorLabel();
  }
  _hasOptionalLabel() {
    return this.optional && this.state !== "error";
  }
  _hasErrorLabel() {
    return this.state === "error";
  }
  static ɵfac = function MatStepHeader_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatStepHeader)();
  };
  static ɵcmp = ɵɵdefineComponent({
    type: _MatStepHeader,
    selectors: [["mat-step-header"]],
    hostAttrs: ["role", "", 1, "mat-step-header"],
    hostVars: 4,
    hostBindings: function MatStepHeader_HostBindings(rf, ctx) {
      if (rf & 2) {
        ɵɵclassMap("mat-" + (ctx.color || "primary"));
        ɵɵclassProp("mat-step-header-empty-label", ctx._hasEmptyLabel());
      }
    },
    inputs: {
      state: "state",
      label: "label",
      errorMessage: "errorMessage",
      iconOverrides: "iconOverrides",
      index: "index",
      selected: "selected",
      active: "active",
      optional: "optional",
      disableRipple: "disableRipple",
      color: "color"
    },
    features: [ɵɵInheritDefinitionFeature],
    decls: 10,
    vars: 17,
    consts: [["matRipple", "", 1, "mat-step-header-ripple", "mat-focus-indicator", 3, "matRippleTrigger", "matRippleDisabled"], [1, "mat-step-icon-content"], [3, "ngTemplateOutlet", "ngTemplateOutletContext"], [1, "mat-step-label"], [1, "mat-step-text-label"], [1, "mat-step-optional"], [1, "mat-step-sub-label-error"], ["aria-hidden", "true"], [1, "cdk-visually-hidden"], [3, "ngTemplateOutlet"]],
    template: function MatStepHeader_Template(rf, ctx) {
      if (rf & 1) {
        ɵɵelement(0, "div", 0);
        ɵɵelementStart(1, "div")(2, "div", 1);
        ɵɵconditionalCreate(3, MatStepHeader_Conditional_3_Template, 1, 6, "ng-container", 2)(4, MatStepHeader_Conditional_4_Template, 2, 1);
        ɵɵelementEnd()();
        ɵɵelementStart(5, "div", 3);
        ɵɵconditionalCreate(6, MatStepHeader_Conditional_6_Template, 2, 1, "div", 4)(7, MatStepHeader_Conditional_7_Template, 2, 1, "div", 4);
        ɵɵconditionalCreate(8, MatStepHeader_Conditional_8_Template, 2, 1, "div", 5);
        ɵɵconditionalCreate(9, MatStepHeader_Conditional_9_Template, 2, 1, "div", 6);
        ɵɵelementEnd();
      }
      if (rf & 2) {
        let tmp_8_0;
        ɵɵproperty("matRippleTrigger", ctx._getHostElement())("matRippleDisabled", ctx.disableRipple);
        ɵɵadvance();
        ɵɵclassMap(ɵɵinterpolate1("mat-step-icon-state-", ctx.state, " mat-step-icon"));
        ɵɵclassProp("mat-step-icon-selected", ctx.selected);
        ɵɵadvance(2);
        ɵɵconditional(ctx.iconOverrides && ctx.iconOverrides[ctx.state] ? 3 : 4);
        ɵɵadvance(2);
        ɵɵclassProp("mat-step-label-active", ctx.active)("mat-step-label-selected", ctx.selected)("mat-step-label-error", ctx.state == "error");
        ɵɵadvance();
        ɵɵconditional((tmp_8_0 = ctx._templateLabel()) ? 6 : ctx._stringLabel() ? 7 : -1, tmp_8_0);
        ɵɵadvance(2);
        ɵɵconditional(ctx._hasOptionalLabel() ? 8 : -1);
        ɵɵadvance();
        ɵɵconditional(ctx._hasErrorLabel() ? 9 : -1);
      }
    },
    dependencies: [MatRipple, NgTemplateOutlet, MatIcon],
    styles: ['.mat-step-header {\n  overflow: hidden;\n  outline: none;\n  cursor: pointer;\n  position: relative;\n  box-sizing: content-box;\n  -webkit-tap-highlight-color: transparent;\n}\n.mat-step-header:focus-visible .mat-focus-indicator::before {\n  content: "";\n}\n.mat-step-header:hover[aria-disabled=true] {\n  cursor: default;\n}\n.mat-step-header:hover:not([aria-disabled]), .mat-step-header:hover[aria-disabled=false] {\n  background-color: var(--mat-stepper-header-hover-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-hover-state-layer-opacity) * 100%), transparent));\n  border-radius: var(--mat-stepper-header-hover-state-layer-shape, var(--mat-sys-corner-medium));\n}\n.mat-step-header.cdk-keyboard-focused, .mat-step-header.cdk-program-focused {\n  background-color: var(--mat-stepper-header-focus-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-focus-state-layer-opacity) * 100%), transparent));\n  border-radius: var(--mat-stepper-header-focus-state-layer-shape, var(--mat-sys-corner-medium));\n}\n@media (hover: none) {\n  .mat-step-header:hover {\n    background: none;\n  }\n}\n@media (forced-colors: active) {\n  .mat-step-header {\n    outline: solid 1px;\n  }\n  .mat-step-header[aria-selected=true] .mat-step-label {\n    text-decoration: underline;\n  }\n  .mat-step-header[aria-disabled=true] {\n    outline-color: GrayText;\n  }\n  .mat-step-header[aria-disabled=true] .mat-step-label,\n  .mat-step-header[aria-disabled=true] .mat-step-icon,\n  .mat-step-header[aria-disabled=true] .mat-step-optional {\n    color: GrayText;\n  }\n}\n\n.mat-step-optional {\n  font-size: 12px;\n  color: var(--mat-stepper-header-optional-label-text-color, var(--mat-sys-on-surface-variant));\n}\n\n.mat-step-sub-label-error {\n  font-size: 12px;\n  font-weight: normal;\n}\n\n.mat-step-icon {\n  border-radius: 50%;\n  height: 24px;\n  width: 24px;\n  flex-shrink: 0;\n  position: relative;\n  color: var(--mat-stepper-header-icon-foreground-color, var(--mat-sys-surface));\n  background-color: var(--mat-stepper-header-icon-background-color, var(--mat-sys-on-surface-variant));\n}\n\n.mat-step-icon-content {\n  position: absolute;\n  top: 50%;\n  left: 50%;\n  transform: translate(-50%, -50%);\n  display: flex;\n}\n\n.mat-step-icon .mat-icon {\n  font-size: 16px;\n  height: 16px;\n  width: 16px;\n}\n\n.mat-step-icon-state-error {\n  background-color: var(--mat-stepper-header-error-state-icon-background-color, transparent);\n  color: var(--mat-stepper-header-error-state-icon-foreground-color, var(--mat-sys-error));\n}\n.mat-step-icon-state-error .mat-icon {\n  font-size: 24px;\n  height: 24px;\n  width: 24px;\n}\n\n.mat-step-label {\n  display: inline-block;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  min-width: 50px;\n  vertical-align: middle;\n  font-family: var(--mat-stepper-header-label-text-font, var(--mat-sys-title-small-font));\n  font-size: var(--mat-stepper-header-label-text-size, var(--mat-sys-title-small-size));\n  font-weight: var(--mat-stepper-header-label-text-weight, var(--mat-sys-title-small-weight));\n  color: var(--mat-stepper-header-label-text-color, var(--mat-sys-on-surface-variant));\n}\n.mat-step-label.mat-step-label-active {\n  color: var(--mat-stepper-header-selected-state-label-text-color, var(--mat-sys-on-surface-variant));\n}\n.mat-step-label.mat-step-label-error {\n  color: var(--mat-stepper-header-error-state-label-text-color, var(--mat-sys-error));\n  font-size: var(--mat-stepper-header-error-state-label-text-size, var(--mat-sys-title-small-size));\n}\n.mat-step-label.mat-step-label-selected {\n  font-size: var(--mat-stepper-header-selected-state-label-text-size, var(--mat-sys-title-small-size));\n  font-weight: var(--mat-stepper-header-selected-state-label-text-weight, var(--mat-sys-title-small-weight));\n}\n.mat-step-header-empty-label .mat-step-label {\n  min-width: 0;\n}\n\n.mat-step-text-label {\n  text-overflow: ellipsis;\n  overflow: hidden;\n}\n\n.mat-step-header .mat-step-header-ripple {\n  top: 0;\n  left: 0;\n  right: 0;\n  bottom: 0;\n  position: absolute;\n  pointer-events: none;\n}\n\n.mat-step-icon-selected {\n  background-color: var(--mat-stepper-header-selected-state-icon-background-color, var(--mat-sys-primary));\n  color: var(--mat-stepper-header-selected-state-icon-foreground-color, var(--mat-sys-on-primary));\n}\n\n.mat-step-icon-state-done {\n  background-color: var(--mat-stepper-header-done-state-icon-background-color, var(--mat-sys-primary));\n  color: var(--mat-stepper-header-done-state-icon-foreground-color, var(--mat-sys-on-primary));\n}\n\n.mat-step-icon-state-edit {\n  background-color: var(--mat-stepper-header-edit-state-icon-background-color, var(--mat-sys-primary));\n  color: var(--mat-stepper-header-edit-state-icon-foreground-color, var(--mat-sys-on-primary));\n}\n'],
    encapsulation: 2,
    changeDetection: 0
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatStepHeader, [{
    type: Component,
    args: [{
      selector: "mat-step-header",
      host: {
        "class": "mat-step-header",
        "[class.mat-step-header-empty-label]": "_hasEmptyLabel()",
        "[class]": '"mat-" + (color || "primary")',
        "role": ""
      },
      encapsulation: ViewEncapsulation.None,
      changeDetection: ChangeDetectionStrategy.OnPush,
      imports: [MatRipple, NgTemplateOutlet, MatIcon],
      template: `<div class="mat-step-header-ripple mat-focus-indicator" matRipple
     [matRippleTrigger]="_getHostElement()"
     [matRippleDisabled]="disableRipple"></div>

<div class="mat-step-icon-state-{{state}} mat-step-icon" [class.mat-step-icon-selected]="selected">
  <div class="mat-step-icon-content">
    @if (iconOverrides && iconOverrides[state]) {
      <ng-container
        [ngTemplateOutlet]="iconOverrides[state]"
        [ngTemplateOutletContext]="{index, active, optional}"></ng-container>
    } @else {
      @switch (state) {
        @case ('number') {
          <span aria-hidden="true">{{_getDefaultTextForState(state)}}</span>
        }

        @default {
          @if (state === 'done') {
            <span class="cdk-visually-hidden">{{_intl.completedLabel}}</span>
          } @else if (state === 'edit') {
            <span class="cdk-visually-hidden">{{_intl.editableLabel}}</span>
          }

          <mat-icon aria-hidden="true">{{_getDefaultTextForState(state)}}</mat-icon>
        }
      }
    }
  </div>
</div>
<div class="mat-step-label"
     [class.mat-step-label-active]="active"
     [class.mat-step-label-selected]="selected"
     [class.mat-step-label-error]="state == 'error'">
  @if (_templateLabel(); as templateLabel) {
    <!-- If there is a label template, use it. -->
    <div class="mat-step-text-label">
      <ng-container [ngTemplateOutlet]="templateLabel.template"></ng-container>
    </div>
  } @else if (_stringLabel()) {
    <!-- If there is no label template, fall back to the text label. -->
    <div class="mat-step-text-label">{{label}}</div>
  }

  @if (_hasOptionalLabel()) {
    <div class="mat-step-optional">{{_intl.optionalLabel}}</div>
  }

  @if (_hasErrorLabel()) {
    <div class="mat-step-sub-label-error">{{errorMessage}}</div>
  }
</div>

`,
      styles: ['.mat-step-header {\n  overflow: hidden;\n  outline: none;\n  cursor: pointer;\n  position: relative;\n  box-sizing: content-box;\n  -webkit-tap-highlight-color: transparent;\n}\n.mat-step-header:focus-visible .mat-focus-indicator::before {\n  content: "";\n}\n.mat-step-header:hover[aria-disabled=true] {\n  cursor: default;\n}\n.mat-step-header:hover:not([aria-disabled]), .mat-step-header:hover[aria-disabled=false] {\n  background-color: var(--mat-stepper-header-hover-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-hover-state-layer-opacity) * 100%), transparent));\n  border-radius: var(--mat-stepper-header-hover-state-layer-shape, var(--mat-sys-corner-medium));\n}\n.mat-step-header.cdk-keyboard-focused, .mat-step-header.cdk-program-focused {\n  background-color: var(--mat-stepper-header-focus-state-layer-color, color-mix(in srgb, var(--mat-sys-on-surface) calc(var(--mat-sys-focus-state-layer-opacity) * 100%), transparent));\n  border-radius: var(--mat-stepper-header-focus-state-layer-shape, var(--mat-sys-corner-medium));\n}\n@media (hover: none) {\n  .mat-step-header:hover {\n    background: none;\n  }\n}\n@media (forced-colors: active) {\n  .mat-step-header {\n    outline: solid 1px;\n  }\n  .mat-step-header[aria-selected=true] .mat-step-label {\n    text-decoration: underline;\n  }\n  .mat-step-header[aria-disabled=true] {\n    outline-color: GrayText;\n  }\n  .mat-step-header[aria-disabled=true] .mat-step-label,\n  .mat-step-header[aria-disabled=true] .mat-step-icon,\n  .mat-step-header[aria-disabled=true] .mat-step-optional {\n    color: GrayText;\n  }\n}\n\n.mat-step-optional {\n  font-size: 12px;\n  color: var(--mat-stepper-header-optional-label-text-color, var(--mat-sys-on-surface-variant));\n}\n\n.mat-step-sub-label-error {\n  font-size: 12px;\n  font-weight: normal;\n}\n\n.mat-step-icon {\n  border-radius: 50%;\n  height: 24px;\n  width: 24px;\n  flex-shrink: 0;\n  position: relative;\n  color: var(--mat-stepper-header-icon-foreground-color, var(--mat-sys-surface));\n  background-color: var(--mat-stepper-header-icon-background-color, var(--mat-sys-on-surface-variant));\n}\n\n.mat-step-icon-content {\n  position: absolute;\n  top: 50%;\n  left: 50%;\n  transform: translate(-50%, -50%);\n  display: flex;\n}\n\n.mat-step-icon .mat-icon {\n  font-size: 16px;\n  height: 16px;\n  width: 16px;\n}\n\n.mat-step-icon-state-error {\n  background-color: var(--mat-stepper-header-error-state-icon-background-color, transparent);\n  color: var(--mat-stepper-header-error-state-icon-foreground-color, var(--mat-sys-error));\n}\n.mat-step-icon-state-error .mat-icon {\n  font-size: 24px;\n  height: 24px;\n  width: 24px;\n}\n\n.mat-step-label {\n  display: inline-block;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  min-width: 50px;\n  vertical-align: middle;\n  font-family: var(--mat-stepper-header-label-text-font, var(--mat-sys-title-small-font));\n  font-size: var(--mat-stepper-header-label-text-size, var(--mat-sys-title-small-size));\n  font-weight: var(--mat-stepper-header-label-text-weight, var(--mat-sys-title-small-weight));\n  color: var(--mat-stepper-header-label-text-color, var(--mat-sys-on-surface-variant));\n}\n.mat-step-label.mat-step-label-active {\n  color: var(--mat-stepper-header-selected-state-label-text-color, var(--mat-sys-on-surface-variant));\n}\n.mat-step-label.mat-step-label-error {\n  color: var(--mat-stepper-header-error-state-label-text-color, var(--mat-sys-error));\n  font-size: var(--mat-stepper-header-error-state-label-text-size, var(--mat-sys-title-small-size));\n}\n.mat-step-label.mat-step-label-selected {\n  font-size: var(--mat-stepper-header-selected-state-label-text-size, var(--mat-sys-title-small-size));\n  font-weight: var(--mat-stepper-header-selected-state-label-text-weight, var(--mat-sys-title-small-weight));\n}\n.mat-step-header-empty-label .mat-step-label {\n  min-width: 0;\n}\n\n.mat-step-text-label {\n  text-overflow: ellipsis;\n  overflow: hidden;\n}\n\n.mat-step-header .mat-step-header-ripple {\n  top: 0;\n  left: 0;\n  right: 0;\n  bottom: 0;\n  position: absolute;\n  pointer-events: none;\n}\n\n.mat-step-icon-selected {\n  background-color: var(--mat-stepper-header-selected-state-icon-background-color, var(--mat-sys-primary));\n  color: var(--mat-stepper-header-selected-state-icon-foreground-color, var(--mat-sys-on-primary));\n}\n\n.mat-step-icon-state-done {\n  background-color: var(--mat-stepper-header-done-state-icon-background-color, var(--mat-sys-primary));\n  color: var(--mat-stepper-header-done-state-icon-foreground-color, var(--mat-sys-on-primary));\n}\n\n.mat-step-icon-state-edit {\n  background-color: var(--mat-stepper-header-edit-state-icon-background-color, var(--mat-sys-primary));\n  color: var(--mat-stepper-header-edit-state-icon-foreground-color, var(--mat-sys-on-primary));\n}\n']
    }]
  }], () => [], {
    state: [{
      type: Input
    }],
    label: [{
      type: Input
    }],
    errorMessage: [{
      type: Input
    }],
    iconOverrides: [{
      type: Input
    }],
    index: [{
      type: Input
    }],
    selected: [{
      type: Input
    }],
    active: [{
      type: Input
    }],
    optional: [{
      type: Input
    }],
    disableRipple: [{
      type: Input
    }],
    color: [{
      type: Input
    }]
  });
})();
var MatStepperIcon = class _MatStepperIcon {
  templateRef = inject(TemplateRef);
  name;
  constructor() {
  }
  static ɵfac = function MatStepperIcon_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatStepperIcon)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _MatStepperIcon,
    selectors: [["ng-template", "matStepperIcon", ""]],
    inputs: {
      name: [0, "matStepperIcon", "name"]
    }
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatStepperIcon, [{
    type: Directive,
    args: [{
      selector: "ng-template[matStepperIcon]"
    }]
  }], () => [], {
    name: [{
      type: Input,
      args: ["matStepperIcon"]
    }]
  });
})();
var MatStepContent = class _MatStepContent {
  _template = inject(TemplateRef);
  constructor() {
  }
  static ɵfac = function MatStepContent_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatStepContent)();
  };
  static ɵdir = ɵɵdefineDirective({
    type: _MatStepContent,
    selectors: [["ng-template", "matStepContent", ""]]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatStepContent, [{
    type: Directive,
    args: [{
      selector: "ng-template[matStepContent]"
    }]
  }], () => [], null);
})();
var MatStep = class _MatStep extends CdkStep {
  _errorStateMatcher = inject(ErrorStateMatcher, {
    skipSelf: true
  });
  _viewContainerRef = inject(ViewContainerRef);
  _isSelected = Subscription.EMPTY;
  stepLabel = void 0;
  color;
  _lazyContent;
  _portal;
  ngAfterContentInit() {
    this._isSelected = this._stepper.steps.changes.pipe(switchMap(() => {
      return this._stepper.selectionChange.pipe(map((event) => event.selectedStep === this), startWith(this._stepper.selected === this));
    })).subscribe((isSelected) => {
      if (isSelected && this._lazyContent && !this._portal) {
        this._portal = new TemplatePortal(this._lazyContent._template, this._viewContainerRef);
      }
    });
  }
  ngOnDestroy() {
    this._isSelected.unsubscribe();
  }
  isErrorState(control, form) {
    const originalErrorState = this._errorStateMatcher.isErrorState(control, form);
    const customErrorState = !!(control && control.invalid && this.interacted);
    return originalErrorState || customErrorState;
  }
  static ɵfac = /* @__PURE__ */ (() => {
    let ɵMatStep_BaseFactory;
    return function MatStep_Factory(__ngFactoryType__) {
      return (ɵMatStep_BaseFactory || (ɵMatStep_BaseFactory = ɵɵgetInheritedFactory(_MatStep)))(__ngFactoryType__ || _MatStep);
    };
  })();
  static ɵcmp = ɵɵdefineComponent({
    type: _MatStep,
    selectors: [["mat-step"]],
    contentQueries: function MatStep_ContentQueries(rf, ctx, dirIndex) {
      if (rf & 1) {
        ɵɵcontentQuery(dirIndex, MatStepLabel, 5)(dirIndex, MatStepContent, 5);
      }
      if (rf & 2) {
        let _t;
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx.stepLabel = _t.first);
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._lazyContent = _t.first);
      }
    },
    hostAttrs: ["hidden", ""],
    inputs: {
      color: "color"
    },
    exportAs: ["matStep"],
    features: [ɵɵProvidersFeature([{
      provide: ErrorStateMatcher,
      useExisting: _MatStep
    }, {
      provide: CdkStep,
      useExisting: _MatStep
    }]), ɵɵInheritDefinitionFeature],
    ngContentSelectors: _c1,
    decls: 1,
    vars: 0,
    consts: [[3, "cdkPortalOutlet"]],
    template: function MatStep_Template(rf, ctx) {
      if (rf & 1) {
        ɵɵprojectionDef();
        ɵɵtemplate(0, MatStep_ng_template_0_Template, 2, 1, "ng-template");
      }
    },
    dependencies: [CdkPortalOutlet],
    encapsulation: 2,
    changeDetection: 0
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatStep, [{
    type: Component,
    args: [{
      selector: "mat-step",
      providers: [{
        provide: ErrorStateMatcher,
        useExisting: MatStep
      }, {
        provide: CdkStep,
        useExisting: MatStep
      }],
      encapsulation: ViewEncapsulation.None,
      exportAs: "matStep",
      changeDetection: ChangeDetectionStrategy.OnPush,
      imports: [CdkPortalOutlet],
      host: {
        "hidden": ""
      },
      template: '<ng-template>\n  <ng-content></ng-content>\n  <ng-template [cdkPortalOutlet]="_portal"></ng-template>\n</ng-template>\n'
    }]
  }], null, {
    stepLabel: [{
      type: ContentChild,
      args: [MatStepLabel]
    }],
    color: [{
      type: Input
    }],
    _lazyContent: [{
      type: ContentChild,
      args: [MatStepContent, {
        static: false
      }]
    }]
  });
})();
var MatStepper = class _MatStepper extends CdkStepper {
  _ngZone = inject(NgZone);
  _renderer = inject(Renderer2);
  _animationsDisabled = _animationsDisabled();
  _cleanupTransition;
  _isAnimating = signal(false, ...ngDevMode ? [{
    debugName: "_isAnimating"
  }] : []);
  _stepHeader = void 0;
  _animatedContainers;
  _steps = void 0;
  steps = new QueryList();
  _icons;
  animationDone = new EventEmitter();
  disableRipple = false;
  color;
  labelPosition = "end";
  headerPosition = "top";
  headerPrefix = input(null, ...ngDevMode ? [{
    debugName: "headerPrefix"
  }] : []);
  _iconOverrides = {};
  get animationDuration() {
    return this._animationDuration;
  }
  set animationDuration(value) {
    this._animationDuration = /^\d+$/.test(value) ? value + "ms" : value;
  }
  _animationDuration = "";
  _isServer = !inject(Platform).isBrowser;
  constructor() {
    super();
    const elementRef = inject(ElementRef);
    const nodeName = elementRef.nativeElement.nodeName.toLowerCase();
    this.orientation = nodeName === "mat-vertical-stepper" ? "vertical" : "horizontal";
  }
  ngAfterContentInit() {
    super.ngAfterContentInit();
    this._icons.forEach(({
      name,
      templateRef
    }) => this._iconOverrides[name] = templateRef);
    this.steps.changes.pipe(takeUntil(this._destroyed)).subscribe(() => this._stateChanged());
    this.selectedIndexChange.pipe(takeUntil(this._destroyed)).subscribe(() => {
      const duration = this._getAnimationDuration();
      if (duration === "0ms" || duration === "0s") {
        this._onAnimationDone();
      } else {
        this._isAnimating.set(true);
      }
    });
    this._ngZone.runOutsideAngular(() => {
      if (!this._animationsDisabled) {
        setTimeout(() => {
          this._elementRef.nativeElement.classList.add("mat-stepper-animations-enabled");
          this._cleanupTransition = this._renderer.listen(this._elementRef.nativeElement, "transitionend", this._handleTransitionend);
        }, 200);
      }
    });
  }
  ngAfterViewInit() {
    super.ngAfterViewInit();
    if (typeof queueMicrotask === "function") {
      let hasEmittedInitial = false;
      this._animatedContainers.changes.pipe(startWith(null), takeUntil(this._destroyed)).subscribe(() => queueMicrotask(() => {
        if (!hasEmittedInitial) {
          hasEmittedInitial = true;
          this.animationDone.emit();
        }
        this._stateChanged();
      }));
    }
  }
  ngOnDestroy() {
    super.ngOnDestroy();
    this._cleanupTransition?.();
  }
  _getAnimationDuration() {
    if (this._animationsDisabled) {
      return "0ms";
    }
    if (this.animationDuration) {
      return this.animationDuration;
    }
    return this.orientation === "horizontal" ? "500ms" : "225ms";
  }
  _handleTransitionend = (event) => {
    const target = event.target;
    if (!target) {
      return;
    }
    const isHorizontalActiveElement = this.orientation === "horizontal" && event.propertyName === "transform" && target.classList.contains("mat-horizontal-stepper-content-current");
    const isVerticalActiveElement = this.orientation === "vertical" && event.propertyName === "grid-template-rows" && target.classList.contains("mat-vertical-content-container-active");
    const shouldEmit = (isHorizontalActiveElement || isVerticalActiveElement) && this._animatedContainers.find((ref) => ref.nativeElement === target);
    if (shouldEmit) {
      this._onAnimationDone();
    }
  };
  _onAnimationDone() {
    this._isAnimating.set(false);
    this.animationDone.emit();
  }
  static ɵfac = function MatStepper_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatStepper)();
  };
  static ɵcmp = ɵɵdefineComponent({
    type: _MatStepper,
    selectors: [["mat-stepper"], ["mat-vertical-stepper"], ["mat-horizontal-stepper"], ["", "matStepper", ""]],
    contentQueries: function MatStepper_ContentQueries(rf, ctx, dirIndex) {
      if (rf & 1) {
        ɵɵcontentQuery(dirIndex, MatStep, 5)(dirIndex, MatStepperIcon, 5);
      }
      if (rf & 2) {
        let _t;
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._steps = _t);
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._icons = _t);
      }
    },
    viewQuery: function MatStepper_Query(rf, ctx) {
      if (rf & 1) {
        ɵɵviewQuery(MatStepHeader, 5)(_c2, 5);
      }
      if (rf & 2) {
        let _t;
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._stepHeader = _t);
        ɵɵqueryRefresh(_t = ɵɵloadQuery()) && (ctx._animatedContainers = _t);
      }
    },
    hostVars: 14,
    hostBindings: function MatStepper_HostBindings(rf, ctx) {
      if (rf & 2) {
        ɵɵstyleProp("--mat-stepper-animation-duration", ctx._getAnimationDuration());
        ɵɵclassProp("mat-stepper-horizontal", ctx.orientation === "horizontal")("mat-stepper-vertical", ctx.orientation === "vertical")("mat-stepper-label-position-end", ctx.orientation === "horizontal" && ctx.labelPosition == "end")("mat-stepper-label-position-bottom", ctx.orientation === "horizontal" && ctx.labelPosition == "bottom")("mat-stepper-header-position-bottom", ctx.headerPosition === "bottom")("mat-stepper-animating", ctx._isAnimating());
      }
    },
    inputs: {
      disableRipple: "disableRipple",
      color: "color",
      labelPosition: "labelPosition",
      headerPosition: "headerPosition",
      headerPrefix: [1, "headerPrefix"],
      animationDuration: "animationDuration"
    },
    outputs: {
      animationDone: "animationDone"
    },
    exportAs: ["matStepper", "matVerticalStepper", "matHorizontalStepper"],
    features: [ɵɵProvidersFeature([{
      provide: CdkStepper,
      useExisting: _MatStepper
    }]), ɵɵInheritDefinitionFeature],
    ngContentSelectors: _c1,
    decls: 7,
    vars: 2,
    consts: [["stepTemplate", ""], ["horizontalStepsTemplate", ""], ["animatedContainer", ""], [1, "mat-horizontal-stepper-wrapper"], [1, "mat-vertical-stepper-wrapper"], [1, "mat-horizontal-stepper-header-wrapper"], [3, "ngTemplateOutlet", "ngTemplateOutletContext"], [1, "mat-horizontal-content-container"], ["role", "tabpanel", 1, "mat-horizontal-stepper-content", 3, "id", "class"], [3, "ngTemplateOutlet"], ["role", "tabpanel", 1, "mat-horizontal-stepper-content", 3, "id"], [1, "mat-step"], [1, "mat-vertical-content-container"], ["role", "region", 1, "mat-vertical-stepper-content", 3, "id"], [1, "mat-vertical-content"], [3, "click", "keydown", "tabIndex", "id", "index", "state", "label", "selected", "active", "optional", "errorMessage", "iconOverrides", "disableRipple", "color"], ["aria-orientation", "horizontal", "role", "tablist", 1, "mat-horizontal-stepper-header-container"], [1, "mat-stepper-horizontal-line"]],
    template: function MatStepper_Template(rf, ctx) {
      if (rf & 1) {
        ɵɵprojectionDef();
        ɵɵconditionalCreate(0, MatStepper_Conditional_0_Template, 1, 0);
        ɵɵconditionalCreate(1, MatStepper_Case_1_Template, 6, 1, "div", 3)(2, MatStepper_Case_2_Template, 4, 1, "div", 4);
        ɵɵtemplate(3, MatStepper_ng_template_3_Template, 1, 27, "ng-template", null, 0, ɵɵtemplateRefExtractor)(5, MatStepper_ng_template_5_Template, 3, 0, "ng-template", null, 1, ɵɵtemplateRefExtractor);
      }
      if (rf & 2) {
        let tmp_3_0;
        ɵɵconditional(ctx._isServer ? 0 : -1);
        ɵɵadvance();
        ɵɵconditional((tmp_3_0 = ctx.orientation) === "horizontal" ? 1 : tmp_3_0 === "vertical" ? 2 : -1);
      }
    },
    dependencies: [NgTemplateOutlet, MatStepHeader],
    styles: ['.mat-stepper-vertical,\n.mat-stepper-horizontal {\n  display: block;\n  font-family: var(--mat-stepper-container-text-font, var(--mat-sys-body-medium-font));\n  background: var(--mat-stepper-container-color, var(--mat-sys-surface));\n}\n\n.mat-horizontal-stepper-header-wrapper {\n  align-items: center;\n  display: flex;\n}\n\n.mat-horizontal-stepper-header-container {\n  white-space: nowrap;\n  display: flex;\n  align-items: center;\n  flex-grow: 1;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header-container {\n  align-items: flex-start;\n}\n.mat-stepper-header-position-bottom .mat-horizontal-stepper-header-container {\n  order: 1;\n}\n\n.mat-stepper-horizontal-line {\n  border-top-width: 1px;\n  border-top-style: solid;\n  flex: auto;\n  height: 0;\n  margin: 0 -16px;\n  min-width: 32px;\n  border-top-color: var(--mat-stepper-line-color, var(--mat-sys-outline));\n}\n.mat-stepper-label-position-bottom .mat-stepper-horizontal-line {\n  margin: 0;\n  min-width: 0;\n  position: relative;\n  top: calc(calc((var(--mat-stepper-header-height, 72px) - 24px) / 2) + 12px);\n}\n\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:first-child)::before, [dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:last-child)::before, .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:last-child)::after, [dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:first-child)::after {\n  border-top-width: 1px;\n  border-top-style: solid;\n  content: "";\n  display: inline-block;\n  height: 0;\n  position: absolute;\n  width: calc(50% - 20px);\n}\n\n.mat-horizontal-stepper-header {\n  display: flex;\n  overflow: hidden;\n  align-items: center;\n  padding: 0 24px;\n  height: var(--mat-stepper-header-height, 72px);\n}\n.mat-horizontal-stepper-header .mat-step-icon {\n  margin-right: 8px;\n  flex: none;\n}\n[dir=rtl] .mat-horizontal-stepper-header .mat-step-icon {\n  margin-right: 0;\n  margin-left: 8px;\n}\n.mat-horizontal-stepper-header.mat-step-header-empty-label .mat-step-icon {\n  margin: 0;\n}\n.mat-horizontal-stepper-header::before, .mat-horizontal-stepper-header::after {\n  border-top-color: var(--mat-stepper-line-color, var(--mat-sys-outline));\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header {\n  padding: calc((var(--mat-stepper-header-height, 72px) - 24px) / 2) 24px;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header::before, .mat-stepper-label-position-bottom .mat-horizontal-stepper-header::after {\n  top: calc(calc((var(--mat-stepper-header-height, 72px) - 24px) / 2) + 12px);\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header {\n  box-sizing: border-box;\n  flex-direction: column;\n  height: auto;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:last-child)::after, [dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:first-child)::after {\n  right: 0;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:first-child)::before, [dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:last-child)::before {\n  left: 0;\n}\n[dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:last-child::before, [dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:first-child::after {\n  display: none;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header .mat-step-icon {\n  margin-right: 0;\n  margin-left: 0;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header .mat-step-label {\n  padding: 16px 0 0 0;\n  text-align: center;\n  width: 100%;\n}\n\n.mat-vertical-stepper-header {\n  display: flex;\n  align-items: center;\n  height: 24px;\n  padding: calc((var(--mat-stepper-header-height, 72px) - 24px) / 2) 24px;\n}\n.mat-vertical-stepper-header .mat-step-icon {\n  margin-right: 12px;\n}\n[dir=rtl] .mat-vertical-stepper-header .mat-step-icon {\n  margin-right: 0;\n  margin-left: 12px;\n}\n\n.mat-horizontal-stepper-wrapper {\n  display: flex;\n  flex-direction: column;\n}\n\n.mat-horizontal-stepper-content {\n  visibility: hidden;\n  overflow: hidden;\n  outline: 0;\n  height: 0;\n}\n.mat-stepper-animations-enabled .mat-horizontal-stepper-content {\n  transition: transform var(--mat-stepper-animation-duration, 0) cubic-bezier(0.35, 0, 0.25, 1);\n}\n.mat-horizontal-stepper-content.mat-horizontal-stepper-content-previous {\n  transform: translate3d(-100%, 0, 0);\n}\n.mat-horizontal-stepper-content.mat-horizontal-stepper-content-next {\n  transform: translate3d(100%, 0, 0);\n}\n.mat-horizontal-stepper-content.mat-horizontal-stepper-content-current {\n  visibility: visible;\n  transform: none;\n  height: auto;\n}\n.mat-stepper-horizontal:not(.mat-stepper-animating) .mat-horizontal-stepper-content.mat-horizontal-stepper-content-current {\n  overflow: visible;\n}\n\n.mat-horizontal-content-container {\n  overflow: hidden;\n  padding: 0 24px 24px 24px;\n}\n@media (forced-colors: active) {\n  .mat-horizontal-content-container {\n    outline: solid 1px;\n  }\n}\n.mat-stepper-header-position-bottom .mat-horizontal-content-container {\n  padding: 24px 24px 0 24px;\n}\n\n.mat-vertical-content-container {\n  display: grid;\n  grid-template-rows: 0fr;\n  grid-template-columns: 100%;\n  margin-left: 36px;\n  border: 0;\n  position: relative;\n}\n.mat-stepper-animations-enabled .mat-vertical-content-container {\n  transition: grid-template-rows var(--mat-stepper-animation-duration, 0) cubic-bezier(0.4, 0, 0.2, 1);\n}\n.mat-vertical-content-container.mat-vertical-content-container-active {\n  grid-template-rows: 1fr;\n}\n.mat-step:last-child .mat-vertical-content-container {\n  border: none;\n}\n@media (forced-colors: active) {\n  .mat-vertical-content-container {\n    outline: solid 1px;\n  }\n}\n[dir=rtl] .mat-vertical-content-container {\n  margin-left: 0;\n  margin-right: 36px;\n}\n@supports not (grid-template-rows: 0fr) {\n  .mat-vertical-content-container {\n    height: 0;\n  }\n  .mat-vertical-content-container.mat-vertical-content-container-active {\n    height: auto;\n  }\n}\n\n.mat-stepper-vertical-line::before {\n  content: "";\n  position: absolute;\n  left: 0;\n  border-left-width: 1px;\n  border-left-style: solid;\n  border-left-color: var(--mat-stepper-line-color, var(--mat-sys-outline));\n  top: calc(8px - calc((var(--mat-stepper-header-height, 72px) - 24px) / 2));\n  bottom: calc(8px - calc((var(--mat-stepper-header-height, 72px) - 24px) / 2));\n}\n[dir=rtl] .mat-stepper-vertical-line::before {\n  left: auto;\n  right: 0;\n}\n\n.mat-vertical-stepper-content {\n  overflow: hidden;\n  outline: 0;\n  visibility: hidden;\n}\n.mat-stepper-animations-enabled .mat-vertical-stepper-content {\n  transition: visibility var(--mat-stepper-animation-duration, 0) linear;\n}\n.mat-vertical-content-container-active > .mat-vertical-stepper-content {\n  visibility: visible;\n}\n\n.mat-vertical-content {\n  padding: 0 24px 24px 24px;\n}\n'],
    encapsulation: 2,
    changeDetection: 0
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatStepper, [{
    type: Component,
    args: [{
      selector: "mat-stepper, mat-vertical-stepper, mat-horizontal-stepper, [matStepper]",
      exportAs: "matStepper, matVerticalStepper, matHorizontalStepper",
      host: {
        "[class.mat-stepper-horizontal]": 'orientation === "horizontal"',
        "[class.mat-stepper-vertical]": 'orientation === "vertical"',
        "[class.mat-stepper-label-position-end]": 'orientation === "horizontal" && labelPosition == "end"',
        "[class.mat-stepper-label-position-bottom]": 'orientation === "horizontal" && labelPosition == "bottom"',
        "[class.mat-stepper-header-position-bottom]": 'headerPosition === "bottom"',
        "[class.mat-stepper-animating]": "_isAnimating()",
        "[style.--mat-stepper-animation-duration]": "_getAnimationDuration()"
      },
      providers: [{
        provide: CdkStepper,
        useExisting: MatStepper
      }],
      encapsulation: ViewEncapsulation.None,
      changeDetection: ChangeDetectionStrategy.OnPush,
      imports: [NgTemplateOutlet, MatStepHeader],
      template: `<!--
  We need to project the content somewhere to avoid hydration errors. Some observations:
  1. This is only necessary on the server.
  2. We get a hydration error if there aren't any nodes after the \`ng-content\`.
  3. We get a hydration error if \`ng-content\` is wrapped in another element.
-->
@if (_isServer) {
  <ng-content/>
}

@switch (orientation) {
  @case ('horizontal') {
    <div class="mat-horizontal-stepper-wrapper">
      @if (headerPrefix()) {
        <div class="mat-horizontal-stepper-header-wrapper">
          <ng-container [ngTemplateOutlet]="headerPrefix()"/>
          <ng-container [ngTemplateOutlet]="horizontalStepsTemplate"
            [ngTemplateOutletContext]="{steps}"/>
        </div>
      } @else {
        <ng-container [ngTemplateOutlet]="horizontalStepsTemplate"
          [ngTemplateOutletContext]="{steps}"/>
      }

      <div class="mat-horizontal-content-container">
        @for (step of steps; track step) {
          <div
            #animatedContainer
            class="mat-horizontal-stepper-content"
            role="tabpanel"
            [id]="_getStepContentId($index)"
            [attr.aria-labelledby]="_getStepLabelId($index)"
            [class]="'mat-horizontal-stepper-content-' + _getAnimationDirection($index)"
            [attr.inert]="selectedIndex === $index ? null : ''">
            <ng-container [ngTemplateOutlet]="step.content"/>
          </div>
        }
      </div>
    </div>
  }

  @case ('vertical') {
    <div class="mat-vertical-stepper-wrapper">
      @if (headerPrefix()) {
        <ng-container [ngTemplateOutlet]="headerPrefix()"/>
      }

      @for (step of steps; track step) {
        <div class="mat-step">
          <ng-container
            [ngTemplateOutlet]="stepTemplate"
            [ngTemplateOutletContext]="{step}"/>
          <div
            #animatedContainer
            class="mat-vertical-content-container"
            [class.mat-stepper-vertical-line]="!$last"
            [class.mat-vertical-content-container-active]="selectedIndex === $index"
            [attr.inert]="selectedIndex === $index ? null : ''">
            <div 
              class="mat-vertical-stepper-content"
              role="region"
              [id]="_getStepContentId($index)"
              [attr.aria-labelledby]="_getStepLabelId($index)">
              <div class="mat-vertical-content">
                <ng-container [ngTemplateOutlet]="step.content"/>
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  }
}

<!-- Common step templating -->
<ng-template let-step="step" #stepTemplate>
  <mat-step-header
    [class.mat-horizontal-stepper-header]="orientation === 'horizontal'"
    [class.mat-vertical-stepper-header]="orientation === 'vertical'"
    (click)="step.select()"
    (keydown)="_onKeydown($event)"
    [tabIndex]="_getFocusIndex() === step.index() ? 0 : -1"
    [id]="_getStepLabelId(step.index())"
    [attr.role]="orientation === 'horizontal' ? 'tab' : 'button'"
    [attr.aria-posinset]="orientation === 'horizontal' ? step.index() + 1 : null"
    [attr.aria-setsize]="orientation === 'horizontal' ? steps.length : null"
    [attr.aria-selected]="orientation === 'horizontal' ? step.isSelected() : null"
    [attr.aria-current]="orientation === 'vertical' && step.isSelected() ? 'step' : null"
    [attr.aria-disabled]="orientation === 'vertical' && step.isSelected() ? 'true' : null"
    [attr.aria-expanded]="orientation === 'vertical' ? step.isSelected() : null"
    [attr.aria-controls]="_getStepContentId(step.index())"
    [attr.aria-label]="step.ariaLabel || null"
    [attr.aria-labelledby]="(!step.ariaLabel && step.ariaLabelledby) ? step.ariaLabelledby : null"
    [attr.aria-disabled]="step.isNavigable() ? null : true"
    [index]="step.index()"
    [state]="step.indicatorType()"
    [label]="step.stepLabel || step.label"
    [selected]="step.isSelected()"
    [active]="step.isNavigable()"
    [optional]="step.optional"
    [errorMessage]="step.errorMessage"
    [iconOverrides]="_iconOverrides"
    [disableRipple]="disableRipple || !step.isNavigable()"
    [color]="step.color || color"/>
</ng-template>

<ng-template #horizontalStepsTemplate let-steps="steps">
  <div 
    aria-orientation="horizontal"
    class="mat-horizontal-stepper-header-container" 
    role="tablist">
    @for (step of steps; track step) {
      <ng-container
        [ngTemplateOutlet]="stepTemplate"
        [ngTemplateOutletContext]="{step}"/>
      @if (!$last) {
        <div class="mat-stepper-horizontal-line"></div>
      }
    }
  </div>
</ng-template>
`,
      styles: ['.mat-stepper-vertical,\n.mat-stepper-horizontal {\n  display: block;\n  font-family: var(--mat-stepper-container-text-font, var(--mat-sys-body-medium-font));\n  background: var(--mat-stepper-container-color, var(--mat-sys-surface));\n}\n\n.mat-horizontal-stepper-header-wrapper {\n  align-items: center;\n  display: flex;\n}\n\n.mat-horizontal-stepper-header-container {\n  white-space: nowrap;\n  display: flex;\n  align-items: center;\n  flex-grow: 1;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header-container {\n  align-items: flex-start;\n}\n.mat-stepper-header-position-bottom .mat-horizontal-stepper-header-container {\n  order: 1;\n}\n\n.mat-stepper-horizontal-line {\n  border-top-width: 1px;\n  border-top-style: solid;\n  flex: auto;\n  height: 0;\n  margin: 0 -16px;\n  min-width: 32px;\n  border-top-color: var(--mat-stepper-line-color, var(--mat-sys-outline));\n}\n.mat-stepper-label-position-bottom .mat-stepper-horizontal-line {\n  margin: 0;\n  min-width: 0;\n  position: relative;\n  top: calc(calc((var(--mat-stepper-header-height, 72px) - 24px) / 2) + 12px);\n}\n\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:first-child)::before, [dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:last-child)::before, .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:last-child)::after, [dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:first-child)::after {\n  border-top-width: 1px;\n  border-top-style: solid;\n  content: "";\n  display: inline-block;\n  height: 0;\n  position: absolute;\n  width: calc(50% - 20px);\n}\n\n.mat-horizontal-stepper-header {\n  display: flex;\n  overflow: hidden;\n  align-items: center;\n  padding: 0 24px;\n  height: var(--mat-stepper-header-height, 72px);\n}\n.mat-horizontal-stepper-header .mat-step-icon {\n  margin-right: 8px;\n  flex: none;\n}\n[dir=rtl] .mat-horizontal-stepper-header .mat-step-icon {\n  margin-right: 0;\n  margin-left: 8px;\n}\n.mat-horizontal-stepper-header.mat-step-header-empty-label .mat-step-icon {\n  margin: 0;\n}\n.mat-horizontal-stepper-header::before, .mat-horizontal-stepper-header::after {\n  border-top-color: var(--mat-stepper-line-color, var(--mat-sys-outline));\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header {\n  padding: calc((var(--mat-stepper-header-height, 72px) - 24px) / 2) 24px;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header::before, .mat-stepper-label-position-bottom .mat-horizontal-stepper-header::after {\n  top: calc(calc((var(--mat-stepper-header-height, 72px) - 24px) / 2) + 12px);\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header {\n  box-sizing: border-box;\n  flex-direction: column;\n  height: auto;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:last-child)::after, [dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:first-child)::after {\n  right: 0;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:first-child)::before, [dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:not(:last-child)::before {\n  left: 0;\n}\n[dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:last-child::before, [dir=rtl] .mat-stepper-label-position-bottom .mat-horizontal-stepper-header:first-child::after {\n  display: none;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header .mat-step-icon {\n  margin-right: 0;\n  margin-left: 0;\n}\n.mat-stepper-label-position-bottom .mat-horizontal-stepper-header .mat-step-label {\n  padding: 16px 0 0 0;\n  text-align: center;\n  width: 100%;\n}\n\n.mat-vertical-stepper-header {\n  display: flex;\n  align-items: center;\n  height: 24px;\n  padding: calc((var(--mat-stepper-header-height, 72px) - 24px) / 2) 24px;\n}\n.mat-vertical-stepper-header .mat-step-icon {\n  margin-right: 12px;\n}\n[dir=rtl] .mat-vertical-stepper-header .mat-step-icon {\n  margin-right: 0;\n  margin-left: 12px;\n}\n\n.mat-horizontal-stepper-wrapper {\n  display: flex;\n  flex-direction: column;\n}\n\n.mat-horizontal-stepper-content {\n  visibility: hidden;\n  overflow: hidden;\n  outline: 0;\n  height: 0;\n}\n.mat-stepper-animations-enabled .mat-horizontal-stepper-content {\n  transition: transform var(--mat-stepper-animation-duration, 0) cubic-bezier(0.35, 0, 0.25, 1);\n}\n.mat-horizontal-stepper-content.mat-horizontal-stepper-content-previous {\n  transform: translate3d(-100%, 0, 0);\n}\n.mat-horizontal-stepper-content.mat-horizontal-stepper-content-next {\n  transform: translate3d(100%, 0, 0);\n}\n.mat-horizontal-stepper-content.mat-horizontal-stepper-content-current {\n  visibility: visible;\n  transform: none;\n  height: auto;\n}\n.mat-stepper-horizontal:not(.mat-stepper-animating) .mat-horizontal-stepper-content.mat-horizontal-stepper-content-current {\n  overflow: visible;\n}\n\n.mat-horizontal-content-container {\n  overflow: hidden;\n  padding: 0 24px 24px 24px;\n}\n@media (forced-colors: active) {\n  .mat-horizontal-content-container {\n    outline: solid 1px;\n  }\n}\n.mat-stepper-header-position-bottom .mat-horizontal-content-container {\n  padding: 24px 24px 0 24px;\n}\n\n.mat-vertical-content-container {\n  display: grid;\n  grid-template-rows: 0fr;\n  grid-template-columns: 100%;\n  margin-left: 36px;\n  border: 0;\n  position: relative;\n}\n.mat-stepper-animations-enabled .mat-vertical-content-container {\n  transition: grid-template-rows var(--mat-stepper-animation-duration, 0) cubic-bezier(0.4, 0, 0.2, 1);\n}\n.mat-vertical-content-container.mat-vertical-content-container-active {\n  grid-template-rows: 1fr;\n}\n.mat-step:last-child .mat-vertical-content-container {\n  border: none;\n}\n@media (forced-colors: active) {\n  .mat-vertical-content-container {\n    outline: solid 1px;\n  }\n}\n[dir=rtl] .mat-vertical-content-container {\n  margin-left: 0;\n  margin-right: 36px;\n}\n@supports not (grid-template-rows: 0fr) {\n  .mat-vertical-content-container {\n    height: 0;\n  }\n  .mat-vertical-content-container.mat-vertical-content-container-active {\n    height: auto;\n  }\n}\n\n.mat-stepper-vertical-line::before {\n  content: "";\n  position: absolute;\n  left: 0;\n  border-left-width: 1px;\n  border-left-style: solid;\n  border-left-color: var(--mat-stepper-line-color, var(--mat-sys-outline));\n  top: calc(8px - calc((var(--mat-stepper-header-height, 72px) - 24px) / 2));\n  bottom: calc(8px - calc((var(--mat-stepper-header-height, 72px) - 24px) / 2));\n}\n[dir=rtl] .mat-stepper-vertical-line::before {\n  left: auto;\n  right: 0;\n}\n\n.mat-vertical-stepper-content {\n  overflow: hidden;\n  outline: 0;\n  visibility: hidden;\n}\n.mat-stepper-animations-enabled .mat-vertical-stepper-content {\n  transition: visibility var(--mat-stepper-animation-duration, 0) linear;\n}\n.mat-vertical-content-container-active > .mat-vertical-stepper-content {\n  visibility: visible;\n}\n\n.mat-vertical-content {\n  padding: 0 24px 24px 24px;\n}\n']
    }]
  }], () => [], {
    _stepHeader: [{
      type: ViewChildren,
      args: [MatStepHeader]
    }],
    _animatedContainers: [{
      type: ViewChildren,
      args: ["animatedContainer"]
    }],
    _steps: [{
      type: ContentChildren,
      args: [MatStep, {
        descendants: true
      }]
    }],
    _icons: [{
      type: ContentChildren,
      args: [MatStepperIcon, {
        descendants: true
      }]
    }],
    animationDone: [{
      type: Output
    }],
    disableRipple: [{
      type: Input
    }],
    color: [{
      type: Input
    }],
    labelPosition: [{
      type: Input
    }],
    headerPosition: [{
      type: Input
    }],
    headerPrefix: [{
      type: Input,
      args: [{
        isSignal: true,
        alias: "headerPrefix",
        required: false
      }]
    }],
    animationDuration: [{
      type: Input
    }]
  });
})();
var MatStepperNext = class _MatStepperNext extends CdkStepperNext {
  static ɵfac = /* @__PURE__ */ (() => {
    let ɵMatStepperNext_BaseFactory;
    return function MatStepperNext_Factory(__ngFactoryType__) {
      return (ɵMatStepperNext_BaseFactory || (ɵMatStepperNext_BaseFactory = ɵɵgetInheritedFactory(_MatStepperNext)))(__ngFactoryType__ || _MatStepperNext);
    };
  })();
  static ɵdir = ɵɵdefineDirective({
    type: _MatStepperNext,
    selectors: [["button", "matStepperNext", ""]],
    hostAttrs: [1, "mat-stepper-next"],
    hostVars: 1,
    hostBindings: function MatStepperNext_HostBindings(rf, ctx) {
      if (rf & 2) {
        ɵɵdomProperty("type", ctx.type);
      }
    },
    features: [ɵɵInheritDefinitionFeature]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatStepperNext, [{
    type: Directive,
    args: [{
      selector: "button[matStepperNext]",
      host: {
        "class": "mat-stepper-next",
        "[type]": "type"
      }
    }]
  }], null, null);
})();
var MatStepperPrevious = class _MatStepperPrevious extends CdkStepperPrevious {
  static ɵfac = /* @__PURE__ */ (() => {
    let ɵMatStepperPrevious_BaseFactory;
    return function MatStepperPrevious_Factory(__ngFactoryType__) {
      return (ɵMatStepperPrevious_BaseFactory || (ɵMatStepperPrevious_BaseFactory = ɵɵgetInheritedFactory(_MatStepperPrevious)))(__ngFactoryType__ || _MatStepperPrevious);
    };
  })();
  static ɵdir = ɵɵdefineDirective({
    type: _MatStepperPrevious,
    selectors: [["button", "matStepperPrevious", ""]],
    hostAttrs: [1, "mat-stepper-previous"],
    hostVars: 1,
    hostBindings: function MatStepperPrevious_HostBindings(rf, ctx) {
      if (rf & 2) {
        ɵɵdomProperty("type", ctx.type);
      }
    },
    features: [ɵɵInheritDefinitionFeature]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatStepperPrevious, [{
    type: Directive,
    args: [{
      selector: "button[matStepperPrevious]",
      host: {
        "class": "mat-stepper-previous",
        "[type]": "type"
      }
    }]
  }], null, null);
})();
var MatStepperModule = class _MatStepperModule {
  static ɵfac = function MatStepperModule_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _MatStepperModule)();
  };
  static ɵmod = ɵɵdefineNgModule({
    type: _MatStepperModule,
    imports: [PortalModule, CdkStepperModule, MatIconModule, MatRippleModule, MatStep, MatStepLabel, MatStepper, MatStepperNext, MatStepperPrevious, MatStepHeader, MatStepperIcon, MatStepContent],
    exports: [BidiModule, MatStep, MatStepLabel, MatStepper, MatStepperNext, MatStepperPrevious, MatStepHeader, MatStepperIcon, MatStepContent]
  });
  static ɵinj = ɵɵdefineInjector({
    providers: [ErrorStateMatcher],
    imports: [PortalModule, CdkStepperModule, MatIconModule, MatRippleModule, MatStepper, MatStepHeader, BidiModule]
  });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(MatStepperModule, [{
    type: NgModule,
    args: [{
      imports: [PortalModule, CdkStepperModule, MatIconModule, MatRippleModule, MatStep, MatStepLabel, MatStepper, MatStepperNext, MatStepperPrevious, MatStepHeader, MatStepperIcon, MatStepContent],
      exports: [BidiModule, MatStep, MatStepLabel, MatStepper, MatStepperNext, MatStepperPrevious, MatStepHeader, MatStepperIcon, MatStepContent],
      providers: [ErrorStateMatcher]
    }]
  }], null, null);
})();
export {
  MatStep,
  MatStepContent,
  MatStepHeader,
  MatStepLabel,
  MatStepper,
  MatStepperIcon,
  MatStepperIntl,
  MatStepperModule,
  MatStepperNext,
  MatStepperPrevious
};
//# sourceMappingURL=@angular_material_stepper.js.map
