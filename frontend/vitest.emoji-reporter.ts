// ------------------------------------------------------------------
// Archivo: vitest.emoji-reporter.ts
// Fecha  : 2026-11-11
// Autor  : gobernalc
// Resumen: Reporter de Vitest que imprime ✅/❌ por cada prueba.
// ------------------------------------------------------------------
import type { Reporter, TestCase } from 'vitest/node';

export default class EmojiReporter implements Reporter {
  onTestCaseResult(testCase: TestCase) {
    const state = testCase.result().state;
    if (state === 'passed') {
      console.log(`✅ ${testCase.name}`);
    } else if (state === 'failed') {
      console.log(`❌ ${testCase.name}`);
    }
  }
}
