import AddIcon from '@mui/icons-material/Add';
import { useTranslation } from 'react-i18next';
import { IllustratedEmptyState } from '../../../shared/components/IllustratedEmptyState';
import { EmptyFlocksIllustration } from '../../../assets/illustrations';

interface FlocksEmptyStateProps {
  onAddClick: () => void;
}

export function FlocksEmptyState({ onAddClick }: FlocksEmptyStateProps) {
  const { t } = useTranslation();

  return (
    <IllustratedEmptyState
      illustration={<EmptyFlocksIllustration aria-label={t('flocks.emptyState.title')} />}
      title={t('flocks.emptyState.title')}
      description={t('flocks.emptyState.message')}
      actionLabel={t('flocks.addFlock')}
      onAction={onAddClick}
      actionIcon={<AddIcon />}
    />
  );
}
