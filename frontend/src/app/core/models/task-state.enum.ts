/** Lifecycle states of a task, matching the numeric values returned by the API. */
export enum TaskState {
  /** The task has not started yet. */
  Pending = 0,

  /** The task is being worked on. */
  InProgress = 1,

  /** The task is finished. */
  Done = 2
}
