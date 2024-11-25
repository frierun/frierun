export default {
    api: {
        input: 'http://localhost:5000/swagger/v1/swagger.json',
        output: {
            target: './src/api/endpoints',
            schemas: './src/api/schemas',
            mode: 'tags',
            client: 'react-query',
            httpClient: 'fetch',
            baseUrl: '/api/v1',
            prettier: true,
            clean: ['src/api/endpoints', 'src/api/schemas'],
            override: {
                contentType: {
                    include: ['application/json'],
                },
                mutator: {
                    path: './src/custom-fetch.ts',
                    name: 'customFetch',
                },
            },
        }
    },
};