// Vitest reporter that prints ✅/❌ for each test.
import type { Reporter, TestCase } from 'vitest/node';

/** Reports each test result as a ✅ or ❌ line in the console. */
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
