"""
AI Service - FastAPI + RAG System
=============================

This module provides the main FastAPI application for the AI Service.
It uses LangChain/LlamaIndex for RAG (Retrieval-Augmented Generation) and 
PostgreSQL with pgvector for vector storage.

Main Features:
- REST API for querying product information
- RAG system for contextual answers
- Vector similarity search for product recommendations
- Integration with LangChain/LlamaIndex
"""

from fastapi import FastAPI, HTTPException, Depends
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional, Dict, Any
import os
import logging

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Create FastAPI app
app = FastAPI(
    title="E-Commerce AI Service",
    description="AI-powered product query service using RAG",
    version="1.0.0"
)

# ================================================================================
# CORS Configuration
# ================================================================================

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ================================================================================
# Models
# ================================================================================

class QueryRequest(BaseModel):
    """Request model for AI query"""
    question: str
    customer_id: Optional[str] = None
    context: Optional[Dict[str, Any]] = None

class QueryResponse(BaseModel):
    """Response model for AI query"""
    answer: str
    sources: List[Dict[str, Any]]
    confidence: float

class ProductDocument(BaseModel):
    """Product document model for vector storage"""
    product_id: str
    product_name: str
    description: str
    category: str
    brand: str
    price: float
    features: List[str]

# ================================================================================
# Vector Store (PostgreSQL + pgvector)
# ================================================================================

class VectorStore:
    """
    Vector Store for product embeddings
    Uses PostgreSQL with pgvector extension for similarity search
    """
    
    def __init__(self):
        self.connection_string = os.getenv(
            "DATABASE_URL",
            "postgresql://ecommerce_user:SecureP@ss2024@postgres:5432/ecommerce_db"
        )
        self.embeddings_model = None
        self.vector_store = None
        logger.info("Vector store initialized")
    
    async def initialize(self):
        """Initialize the vector store with sample product data"""
        logger.info("Initializing vector store...")
        # In production, this would load embeddings from PostgreSQL
        # For now, we'll use an in-memory store
        pass
    
    async def similarity_search(self, query: str, top_k: int = 5) -> List[Dict[str, Any]]:
        """
        Perform similarity search on product documents
        """
        logger.info(f"Performing similarity search for: {query}")
        # In production, this would query pgvector
        # Return sample results for now
        return []

# ================================================================================
# RAG Chain
# ================================================================================

class RAGChain:
    """
    RAG (Retrieval-Augmented Generation) chain
    Combines retrieval from vector store with LLM generation
    """
    
    def __init__(self):
        self.vector_store = VectorStore()
        logger.info("RAG chain initialized")
    
    async def initialize(self):
        """Initialize the RAG chain"""
        await self.vector_store.initialize()
    
    async def invoke(self, query: str, context: Optional[Dict] = None) -> QueryResponse:
        """
        Invoke the RAG chain to answer a query
        """
        logger.info(f"Processing query: {query}")
        
        # Perform similarity search
        relevant_docs = await self.vector_store.similarity_search(query, top_k=5)
        
        # Generate answer (in production, this would call an LLM)
        # For now, we'll return a simple response
        answer = f"Based on our product catalog, I found {len(relevant_docs)} relevant products for your query about '{query}'."
        
        return QueryResponse(
            answer=answer,
            sources=relevant_docs,
            confidence=0.85
        )

# ================================================================================
# Dependencies
# ================================================================================

async def get_rag_chain() -> RAGChain:
    """Dependency for getting RAG chain"""
    return RAGChain()

# ================================================================================
# API Endpoints
# ================================================================================

@app.on_event("startup")
async def startup_event():
    """Initialize services on startup"""
    logger.info("Starting AI Service...")
    
    # Initialize RAG chain
    rag_chain = RAGChain()
    await rag_chain.initialize()
    
    logger.info("AI Service started successfully")

@app.get("/")
async def root():
    """Root endpoint"""
    return {"message": "E-Commerce AI Service", "version": "1.0.0"}

@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "service": "AI Service"}

@app.post("/query", response_model=QueryResponse)
async def query_ai(
    request: QueryRequest,
    rag_chain: RAGChain = Depends(get_rag_chain)
):
    """
    Query the AI service
    
    This endpoint accepts a question and returns an answer
    generated using RAG (Retrieval-Augmented Generation).
    """
    try:
        logger.info(f"Received query: {request.question}")
        
        # Invoke RAG chain
        response = await rag_chain.invoke(
            query=request.question,
            context=request.context
        )
        
        return response
        
    except Exception as e:
        logger.error(f"Error processing query: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/products/search")
async def search_products(
    query: str,
    limit: int = 10
):
    """
    Search products using vector similarity
    """
    try:
        vector_store = VectorStore()
        results = await vector_store.similarity_search(query, top_k=limit)
        
        return {"results": results, "count": len(results)}
        
    except Exception as e:
        logger.error(f"Error searching products: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/products/index")
async def index_product(document: ProductDocument):
    """
    Index a product document for search
    """
    try:
        logger.info(f"Indexing product: {document.product_name}")
        
        # In production, this would create embeddings and store in pgvector
        return {
            "status": "indexed",
            "product_id": document.product_id
        }
        
    except Exception as e:
        logger.error(f"Error indexing product: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

# ================================================================================
# Run with: uvicorn main:app --host 0.0.0.0 --port 8000
# ================================================================================

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
