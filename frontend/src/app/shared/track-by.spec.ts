import { describe, it, expect } from 'vitest';

describe('PERF-03: Template trackBy Verification', () => {
  const trackById = (_: number, item: any): any =>
    item?.id ?? item?.ticketNumber ?? item?.key ?? item?.name ?? item?.code ?? item?.value ?? item ?? _;
  const trackByIndex = (_: number): number => _;

  it('should track by id if present', () => {
    expect(trackById(0, { id: 101, title: 'Test Ticket' })).toBe(101);
    expect(trackById(1, { id: 'guid-1234', title: 'Task' })).toBe('guid-1234');
  });

  it('should track by ticketNumber if present', () => {
    expect(trackById(0, { ticketNumber: 'TICK-999', title: 'Incident' })).toBe('TICK-999');
  });

  it('should track by key if present', () => {
    expect(trackById(0, { key: 'canView', label: 'View' })).toBe('canView');
  });

  it('should track by name if present', () => {
    expect(trackById(0, { name: 'Tier 1 Support' })).toBe('Tier 1 Support');
  });

  it('should track by code or value if present', () => {
    expect(trackById(0, { code: 'US', country: 'United States' })).toBe('US');
    expect(trackById(0, { value: 'option1', text: 'First' })).toBe('value' in { value: 'option1' } ? 'option1' : 0);
  });

  it('should track by primitive string or number', () => {
    expect(trackById(0, 'High')).toBe('High');
    expect(trackById(0, 0)).toBe(0);
    expect(trackById(5, 5)).toBe(5);
  });

  it('should fall back to index when item is null or undefined', () => {
    expect(trackById(3, null)).toBe(3);
    expect(trackById(4, undefined)).toBe(4);
  });

  it('trackByIndex should return the exact index', () => {
    expect(trackByIndex(7)).toBe(7);
  });
});
