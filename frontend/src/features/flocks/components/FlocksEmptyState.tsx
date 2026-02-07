import EggIcon from '@mui/icons-material/Egg';
import AddIcon from '@mui/icons-material/Add';
import { useTranslation } from 'react-i18next';
import { IllustratedEmptyState } from '../../../shared/components/IllustratedEmptyState';

interface FlocksEmptyStateProps {
  onAddClick: () => void;
}

export function FlocksEmptyState({ onAddClick }: FlocksEmptyStateProps) {
  const { t } = useTranslation();

  return (
    <IllustratedEmptyState
      illustration={<EggIcon />}
      title={t('flocks.emptyState.title')}
      description={t('flocks.emptyState.message')}
      actionLabel={t('flocks.addFlock')}
      onAction={onAddClick}
      actionIcon={<AddIcon />}
    />
  );
}
