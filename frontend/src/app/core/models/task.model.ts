import { TaskState } from './task-state.enum';

/** Read-only representation of a task returned by the API. */
export interface Task {
  /** Task identifier. */
  id: number;

  /** Task title. */
  title: string;

  /** Current status of the task. */
  status: TaskState;

  /** Identifier of the assigned user. */
  userId: number;

  /** Name of the assigned user. */
  userName: string;

  /** Creation timestamp in UTC (ISO 8601). */
  createdAt: string;

  /** Last modification timestamp in UTC, when applicable. */
  updatedAt: string | null;

  /** Additional information stored as a JSON document. */
  additionalData: string | null;
}

/** Input data to create a task. */
export interface CreateTaskPayload {
  /** Required task title. */
  title: string;

  /** Identifier of the assigned user. */
  userId: number;

  /** Optional initial status; defaults to Pending when omitted. */
  status?: TaskState;

  /** Optional additional information as a JSON document. */
  additionalData?: string | null;
}

/** Input data to update a task. */
export interface UpdateTaskPayload {
  /** Required task title. */
  title: string;

  /** Identifier of the assigned user. */
  userId: number;

  /** Additional information as a JSON document; null clears the field. */
  additionalData?: string | null;
}

/** Query filters for the task listing. */
export interface TaskFilter {
  /** Filters tasks by status. */
  status?: TaskState;

  /** Filters tasks by assigned user. */
  userId?: number;

  /** Filters tasks by the priority value inside AdditionalData. */
  priority?: string;

  /** Free text matched against the task title. */
  search?: string;

  /** Field used for sorting. */
  sort?: string;

  /** Sort direction. */
  order?: 'asc' | 'desc';

  /** One-based page index. */
  page?: number;

  /** Number of items per page. */
  pageSize?: number;
}
