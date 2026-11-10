import { useQuery } from '@tanstack/react-query'
import { catalogueApi } from '@/shared/api/endpoints'

const STALE = 5 * 60_000

/** Reference lists shared by forms and filters, cached for five minutes. */
export const useCategories = () =>
  useQuery({ queryKey: ['categories'], queryFn: catalogueApi.categories, staleTime: STALE })

export const useLocations = () =>
  useQuery({ queryKey: ['locations'], queryFn: catalogueApi.locations, staleTime: STALE })

export const useFeatures = () =>
  useQuery({ queryKey: ['features'], queryFn: catalogueApi.features, staleTime: STALE })

export const useDepartments = () =>
  useQuery({ queryKey: ['departments'], queryFn: catalogueApi.departments, staleTime: STALE })

export const useConditions = () =>
  useQuery({ queryKey: ['item-conditions'], queryFn: catalogueApi.conditions, staleTime: STALE })

export const useMaintenanceTypes = () =>
  useQuery({
    queryKey: ['maintenance-types'],
    queryFn: catalogueApi.maintenanceTypes,
    staleTime: STALE,
  })
