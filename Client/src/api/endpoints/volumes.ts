/**
 * Generated by orval v7.5.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import { useQuery } from "@tanstack/react-query";
import type {
  DataTag,
  DefinedInitialDataOptions,
  DefinedUseQueryResult,
  QueryFunction,
  QueryKey,
  UndefinedInitialDataOptions,
  UseQueryOptions,
  UseQueryResult,
} from "@tanstack/react-query";
import type { DockerVolume } from "../schemas";
import { customFetch } from "../../custom-fetch";

type SecondParameter<T extends (...args: any) => any> = Parameters<T>[1];

export type getVolumesResponse = {
  data: DockerVolume[];
  status: number;
  headers: Headers;
};

export const getGetVolumesUrl = () => {
  return `/api/v1/volumes`;
};

export const getVolumes = async (
  options?: RequestInit,
): Promise<getVolumesResponse> => {
  return customFetch<getVolumesResponse>(getGetVolumesUrl(), {
    ...options,
    method: "GET",
  });
};

export const getGetVolumesQueryKey = () => {
  return [`/api/v1/volumes`] as const;
};

export const getGetVolumesQueryOptions = <
  TData = Awaited<ReturnType<typeof getVolumes>>,
  TError = unknown,
>(options?: {
  query?: Partial<
    UseQueryOptions<Awaited<ReturnType<typeof getVolumes>>, TError, TData>
  >;
  request?: SecondParameter<typeof customFetch>;
}) => {
  const { query: queryOptions, request: requestOptions } = options ?? {};

  const queryKey = queryOptions?.queryKey ?? getGetVolumesQueryKey();

  const queryFn: QueryFunction<Awaited<ReturnType<typeof getVolumes>>> = ({
    signal,
  }) => getVolumes({ signal, ...requestOptions });

  return { queryKey, queryFn, ...queryOptions } as UseQueryOptions<
    Awaited<ReturnType<typeof getVolumes>>,
    TError,
    TData
  > & { queryKey: DataTag<QueryKey, TData, TError> };
};

export type GetVolumesQueryResult = NonNullable<
  Awaited<ReturnType<typeof getVolumes>>
>;
export type GetVolumesQueryError = unknown;

export function useGetVolumes<
  TData = Awaited<ReturnType<typeof getVolumes>>,
  TError = unknown,
>(options: {
  query: Partial<
    UseQueryOptions<Awaited<ReturnType<typeof getVolumes>>, TError, TData>
  > &
    Pick<
      DefinedInitialDataOptions<
        Awaited<ReturnType<typeof getVolumes>>,
        TError,
        Awaited<ReturnType<typeof getVolumes>>
      >,
      "initialData"
    >;
  request?: SecondParameter<typeof customFetch>;
}): DefinedUseQueryResult<TData, TError> & {
  queryKey: DataTag<QueryKey, TData, TError>;
};
export function useGetVolumes<
  TData = Awaited<ReturnType<typeof getVolumes>>,
  TError = unknown,
>(options?: {
  query?: Partial<
    UseQueryOptions<Awaited<ReturnType<typeof getVolumes>>, TError, TData>
  > &
    Pick<
      UndefinedInitialDataOptions<
        Awaited<ReturnType<typeof getVolumes>>,
        TError,
        Awaited<ReturnType<typeof getVolumes>>
      >,
      "initialData"
    >;
  request?: SecondParameter<typeof customFetch>;
}): UseQueryResult<TData, TError> & {
  queryKey: DataTag<QueryKey, TData, TError>;
};
export function useGetVolumes<
  TData = Awaited<ReturnType<typeof getVolumes>>,
  TError = unknown,
>(options?: {
  query?: Partial<
    UseQueryOptions<Awaited<ReturnType<typeof getVolumes>>, TError, TData>
  >;
  request?: SecondParameter<typeof customFetch>;
}): UseQueryResult<TData, TError> & {
  queryKey: DataTag<QueryKey, TData, TError>;
};

export function useGetVolumes<
  TData = Awaited<ReturnType<typeof getVolumes>>,
  TError = unknown,
>(options?: {
  query?: Partial<
    UseQueryOptions<Awaited<ReturnType<typeof getVolumes>>, TError, TData>
  >;
  request?: SecondParameter<typeof customFetch>;
}): UseQueryResult<TData, TError> & {
  queryKey: DataTag<QueryKey, TData, TError>;
} {
  const queryOptions = getGetVolumesQueryOptions(options);

  const query = useQuery(queryOptions) as UseQueryResult<TData, TError> & {
    queryKey: DataTag<QueryKey, TData, TError>;
  };

  query.queryKey = queryOptions.queryKey;

  return query;
}
