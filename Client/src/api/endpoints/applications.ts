/**
 * Generated by orval v7.6.0 🍺
 * Do not edit manually.
 * Frierun.Server
 * OpenAPI spec version: 1.0
 */
import { useMutation, useQuery } from "@tanstack/react-query";
import type {
  DataTag,
  DefinedInitialDataOptions,
  DefinedUseQueryResult,
  MutationFunction,
  QueryFunction,
  QueryKey,
  UndefinedInitialDataOptions,
  UseMutationOptions,
  UseMutationResult,
  UseQueryOptions,
  UseQueryResult,
} from "@tanstack/react-query";

import type { ApplicationResponse, ProblemDetails } from "../schemas";

import { customFetch } from "../../custom-fetch";

type SecondParameter<T extends (...args: never) => unknown> = Parameters<T>[1];

export type getApplicationsResponse200 = {
  data: ApplicationResponse[];
  status: 200;
};

export type getApplicationsResponseComposite = getApplicationsResponse200;

export type getApplicationsResponse = getApplicationsResponseComposite & {
  headers: Headers;
};

export const getGetApplicationsUrl = () => {
  return `/api/v1/applications`;
};

export const getApplications = async (
  options?: RequestInit,
): Promise<getApplicationsResponse> => {
  return customFetch<getApplicationsResponse>(getGetApplicationsUrl(), {
    ...options,
    method: "GET",
  });
};

export const getGetApplicationsQueryKey = () => {
  return [`/api/v1/applications`] as const;
};

export const getGetApplicationsQueryOptions = <
  TData = Awaited<ReturnType<typeof getApplications>>,
  TError = unknown,
>(options?: {
  query?: Partial<
    UseQueryOptions<Awaited<ReturnType<typeof getApplications>>, TError, TData>
  >;
  request?: SecondParameter<typeof customFetch>;
}) => {
  const { query: queryOptions, request: requestOptions } = options ?? {};

  const queryKey = queryOptions?.queryKey ?? getGetApplicationsQueryKey();

  const queryFn: QueryFunction<Awaited<ReturnType<typeof getApplications>>> = ({
    signal,
  }) => getApplications({ signal, ...requestOptions });

  return { queryKey, queryFn, ...queryOptions } as UseQueryOptions<
    Awaited<ReturnType<typeof getApplications>>,
    TError,
    TData
  > & { queryKey: DataTag<QueryKey, TData, TError> };
};

export type GetApplicationsQueryResult = NonNullable<
  Awaited<ReturnType<typeof getApplications>>
>;
export type GetApplicationsQueryError = unknown;

export function useGetApplications<
  TData = Awaited<ReturnType<typeof getApplications>>,
  TError = unknown,
>(options: {
  query: Partial<
    UseQueryOptions<Awaited<ReturnType<typeof getApplications>>, TError, TData>
  > &
    Pick<
      DefinedInitialDataOptions<
        Awaited<ReturnType<typeof getApplications>>,
        TError,
        Awaited<ReturnType<typeof getApplications>>
      >,
      "initialData"
    >;
  request?: SecondParameter<typeof customFetch>;
}): DefinedUseQueryResult<TData, TError> & {
  queryKey: DataTag<QueryKey, TData, TError>;
};
export function useGetApplications<
  TData = Awaited<ReturnType<typeof getApplications>>,
  TError = unknown,
>(options?: {
  query?: Partial<
    UseQueryOptions<Awaited<ReturnType<typeof getApplications>>, TError, TData>
  > &
    Pick<
      UndefinedInitialDataOptions<
        Awaited<ReturnType<typeof getApplications>>,
        TError,
        Awaited<ReturnType<typeof getApplications>>
      >,
      "initialData"
    >;
  request?: SecondParameter<typeof customFetch>;
}): UseQueryResult<TData, TError> & {
  queryKey: DataTag<QueryKey, TData, TError>;
};
export function useGetApplications<
  TData = Awaited<ReturnType<typeof getApplications>>,
  TError = unknown,
>(options?: {
  query?: Partial<
    UseQueryOptions<Awaited<ReturnType<typeof getApplications>>, TError, TData>
  >;
  request?: SecondParameter<typeof customFetch>;
}): UseQueryResult<TData, TError> & {
  queryKey: DataTag<QueryKey, TData, TError>;
};

export function useGetApplications<
  TData = Awaited<ReturnType<typeof getApplications>>,
  TError = unknown,
>(options?: {
  query?: Partial<
    UseQueryOptions<Awaited<ReturnType<typeof getApplications>>, TError, TData>
  >;
  request?: SecondParameter<typeof customFetch>;
}): UseQueryResult<TData, TError> & {
  queryKey: DataTag<QueryKey, TData, TError>;
} {
  const queryOptions = getGetApplicationsQueryOptions(options);

  const query = useQuery(queryOptions) as UseQueryResult<TData, TError> & {
    queryKey: DataTag<QueryKey, TData, TError>;
  };

  query.queryKey = queryOptions.queryKey;

  return query;
}

export type deleteApplicationsIdResponse202 = {
  data: void;
  status: 202;
};

export type deleteApplicationsIdResponse404 = {
  data: ProblemDetails;
  status: 404;
};

export type deleteApplicationsIdResponseComposite =
  | deleteApplicationsIdResponse202
  | deleteApplicationsIdResponse404;

export type deleteApplicationsIdResponse =
  deleteApplicationsIdResponseComposite & {
    headers: Headers;
  };

export const getDeleteApplicationsIdUrl = (id: string) => {
  return `/api/v1/applications/${id}`;
};

export const deleteApplicationsId = async (
  id: string,
  options?: RequestInit,
): Promise<deleteApplicationsIdResponse> => {
  return customFetch<deleteApplicationsIdResponse>(
    getDeleteApplicationsIdUrl(id),
    {
      ...options,
      method: "DELETE",
    },
  );
};

export const getDeleteApplicationsIdMutationOptions = <
  TError = ProblemDetails,
  TContext = unknown,
>(options?: {
  mutation?: UseMutationOptions<
    Awaited<ReturnType<typeof deleteApplicationsId>>,
    TError,
    { id: string },
    TContext
  >;
  request?: SecondParameter<typeof customFetch>;
}): UseMutationOptions<
  Awaited<ReturnType<typeof deleteApplicationsId>>,
  TError,
  { id: string },
  TContext
> => {
  const mutationKey = ["deleteApplicationsId"];
  const { mutation: mutationOptions, request: requestOptions } = options
    ? options.mutation &&
      "mutationKey" in options.mutation &&
      options.mutation.mutationKey
      ? options
      : { ...options, mutation: { ...options.mutation, mutationKey } }
    : { mutation: { mutationKey }, request: undefined };

  const mutationFn: MutationFunction<
    Awaited<ReturnType<typeof deleteApplicationsId>>,
    { id: string }
  > = (props) => {
    const { id } = props ?? {};

    return deleteApplicationsId(id, requestOptions);
  };

  return { mutationFn, ...mutationOptions };
};

export type DeleteApplicationsIdMutationResult = NonNullable<
  Awaited<ReturnType<typeof deleteApplicationsId>>
>;

export type DeleteApplicationsIdMutationError = ProblemDetails;

export const useDeleteApplicationsId = <
  TError = ProblemDetails,
  TContext = unknown,
>(options?: {
  mutation?: UseMutationOptions<
    Awaited<ReturnType<typeof deleteApplicationsId>>,
    TError,
    { id: string },
    TContext
  >;
  request?: SecondParameter<typeof customFetch>;
}): UseMutationResult<
  Awaited<ReturnType<typeof deleteApplicationsId>>,
  TError,
  { id: string },
  TContext
> => {
  const mutationOptions = getDeleteApplicationsIdMutationOptions(options);

  return useMutation(mutationOptions);
};
