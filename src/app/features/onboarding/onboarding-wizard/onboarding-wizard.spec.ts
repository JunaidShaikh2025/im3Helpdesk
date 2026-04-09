import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OnboardingWizard } from './onboarding-wizard';

describe('OnboardingWizard', () => {
  let component: OnboardingWizard;
  let fixture: ComponentFixture<OnboardingWizard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OnboardingWizard],
    }).compileComponents();

    fixture = TestBed.createComponent(OnboardingWizard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
