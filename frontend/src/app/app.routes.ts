import { Routes } from '@angular/router';

/** Root application routes with lazy-loaded feature shells. */
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  {
    path: 'tasks',
    loadComponent: () =>
      import('./features/tasks/task-list/task-list').then((module) => module.TaskList),
  },
  {
    path: 'users',
    loadComponent: () =>
      import('./features/users/user-list/user-list').then((module) => module.UserList),
  },
  { path: '**', redirectTo: 'tasks' },
];
