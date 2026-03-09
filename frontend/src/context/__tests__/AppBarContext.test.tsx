import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { AppBarProvider, useAppBar } from '../AppBarContext';

function TestConsumer() {
  const { title, onBack, setAppBar, resetAppBar } = useAppBar();
  return (
    <div>
      <span data-testid="title">{title ?? 'null'}</span>
      <span data-testid="has-back">{onBack ? 'yes' : 'no'}</span>
      <button onClick={() => setAppBar({ title: 'New Title' })}>set title</button>
      <button onClick={() => setAppBar({ onBack: () => {} })}>set back</button>
      <button onClick={() => resetAppBar()}>reset</button>
    </div>
  );
}

describe('AppBarContext', () => {
  it('provides null defaults', () => {
    render(
      <AppBarProvider>
        <TestConsumer />
      </AppBarProvider>
    );
    expect(screen.getByTestId('title')).toHaveTextContent('null');
    expect(screen.getByTestId('has-back')).toHaveTextContent('no');
  });

  it('setAppBar updates title', async () => {
    const user = userEvent.setup();
    render(
      <AppBarProvider>
        <TestConsumer />
      </AppBarProvider>
    );
    await user.click(screen.getByText('set title'));
    expect(screen.getByTestId('title')).toHaveTextContent('New Title');
  });

  it('setAppBar updates onBack', async () => {
    const user = userEvent.setup();
    render(
      <AppBarProvider>
        <TestConsumer />
      </AppBarProvider>
    );
    await user.click(screen.getByText('set back'));
    expect(screen.getByTestId('has-back')).toHaveTextContent('yes');
  });

  it('setAppBar merges state (does not wipe other fields)', async () => {
    const user = userEvent.setup();
    render(
      <AppBarProvider>
        <TestConsumer />
      </AppBarProvider>
    );
    await user.click(screen.getByText('set title'));
    await user.click(screen.getByText('set back'));
    expect(screen.getByTestId('title')).toHaveTextContent('New Title');
    expect(screen.getByTestId('has-back')).toHaveTextContent('yes');
  });

  it('resetAppBar clears title and onBack', async () => {
    const user = userEvent.setup();
    render(
      <AppBarProvider>
        <TestConsumer />
      </AppBarProvider>
    );
    await user.click(screen.getByText('set title'));
    await user.click(screen.getByText('set back'));
    await user.click(screen.getByText('reset'));
    expect(screen.getByTestId('title')).toHaveTextContent('null');
    expect(screen.getByTestId('has-back')).toHaveTextContent('no');
  });

  it('useAppBar outside provider returns default no-op values', () => {
    // Default context value — should not throw
    render(<TestConsumer />);
    expect(screen.getByTestId('title')).toHaveTextContent('null');
    expect(screen.getByTestId('has-back')).toHaveTextContent('no');
  });
});
