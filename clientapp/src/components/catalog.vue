<template>
    <div style="display: block;" v-if="isAuthenticated">
        <span>Search: <input v-model="query"/> <button class="btn btn-primary" @click="updateQuery">Search</button></span>
        <div id="app">            
            <ul :style="gridStyle" class="card-list">
                <li id="bookCard" v-for="book in books" v-bind:key="book" class="card-item">
                    <book-card :title=book.title :price=book.price :thumbnail=book.thumbnail :isbn=book.id :description=book.description></book-card>
                </li>
            </ul>
        </div>               
        <span>
          <button class="btn btn-primary" @click="previousPage" :disabled="this.page == 0">Previous Page</button>
          {{this.page}}        
          <button class="btn btn-primary" @click="nextPage" :disabled="this.books.length < this.items_per_page">Next Page</button>
        </span>
    </div>
    <div v-else>
        <p>please login to continue</p>
    </div>
</template>

<script>
import { mapGetters, mapState, mapActions } from 'vuex'
import axios from 'axios'
import bookCard from './book-card.vue';

export default {
  components: { bookCard },
  mounted(){
      this.runQuery();
  },
  data(){
      return{
          cards:[1,2,3,4,5,6,7,8,9],
          numberOfColumns: 3,
          books: {},
          page: 0,
          items_per_page: 6,
          query: '*'
      }
  },
  computed: {
    ...mapState('context', [
      'profile'
    ]),
    ...mapGetters('context', [
      'isAuthenticated'
    ]),
    gridStyle() {
      return {
        gridTemplateColumns: `repeat(${this.numberOfColumns}, minmax(100px, 1fr))`
      }
    }
  },
  methods: {
    addCard() {
      this.cards.push('new-card')
    },
    ...mapActions('context', [
      'logout'
    ]),
    onclick(evt){
        evt.preventDefault();
    },
    previousPage(){
      this.page-=1;
      this.runQuery()
    },
    nextPage(){
      this.page+=1;
      this.runQuery();
    },
    updateQuery(){
      this.page=0;
      this.runQuery();
    },
    runQuery(){
      axios.get("/api/books?page="+this.page+"&pageSize="+this.items_per_page+"&q="+this.query).then(response=>{
          this.books=response.data;
      });
    }
  },  
}
</script>

<style scoped>
.card-list {
  display: grid;
  grid-gap: 1em;
}

.card-item {
  background-color: #fff;  
  padding: 2em;
  border:2px solid #3024b1;
  border-radius: 25px;
}

body {
  background: #20262E;
  padding: 20px;
  font-family: Helvetica;
}

#app {
  background: #fff;
  border-radius: 4px;
  padding: 20px;
  transition: all 0.2s;
}

ul {
  list-style-type: none;
}
</style>