import AddIcon from '@mui/icons-material/Add';
import { useTranslation } from 'react-i18next';
import { IllustratedEmptyState } from '../../../shared/components/IllustratedEmptyState';
import { EmptyCoopsIllustration } from '../../../assets/illustrations';

interface CoopsEmptyStateProps {
  onAddClick: () => void;
}

export function CoopsEmptyState({ onAddClick }: CoopsEmptyStateProps) {
  const { t } = useTranslation();

  return (
    <IllustratedEmptyState
      illustration={<EmptyCoopsIllustration aria-label={t('coops.emptyState.title')} />}
      title={t('coops.emptyState.title')}
      description={t('coops.emptyState.message')}
      actionLabel={t('coops.addCoop')}
      onAction={onAddClick}
      actionIcon={<AddIcon />}
    />
  );
}
