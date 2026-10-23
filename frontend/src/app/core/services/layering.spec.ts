import { describe, it, expect } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';

describe('Frontend Architectural Layering Boundary', () => {
  it('core interceptors must never import from features layer', () => {
    const interceptorPath = path.resolve(__dirname, '../interceptors/auth.interceptor.ts');
    const content = fs.readFileSync(interceptorPath, 'utf-8');
    expect(content).not.toContain('features/auth');
    expect(content).not.toContain('../../features');
  });

  it('core guards must never import from features layer', () => {
    const guardsDir = path.resolve(__dirname, '../guards');
    const files = fs.readdirSync(guardsDir).filter(f => f.endsWith('.ts') && !f.endsWith('.spec.ts'));
    for (const file of files) {
      const content = fs.readFileSync(path.join(guardsDir, file), 'utf-8');
      expect(content).not.toContain('features/auth');
      expect(content).not.toContain('../../features');
    }
  });
});
