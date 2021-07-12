<template>
  <nav class="navbar navbar-expand-md navbar-dark bg-dark shadow">
    <a class="navbar-brand" href="#/">NRedi2Read</a>
    <div class="collapse navbar-collapse" id="main-navbar">      
      <span v-if="isAuthenticated" class="navbar-text mr-2">
        Welcome back, {{ profile.name }}
      </span>
      <form @click="onclick" v-if="isAuthenticated" style="width: 100%">
        <button class="btn btn-primary" type="submit" @click.prevent="logout">Logout</button>
        <button class="btn btn-primary" id="cartButton" type="submit" @click="showCartModal" v-b-modal.prevent.cartModal>Cart</button>
      </form>
      <form v-else style="width: 100%">
        <button @click="onclick" v-b-modal.prevent.loginModal class="btn btn-primary" type="submit">Login</button>
        <button @click="onclick" v-b-modal.prevent.registrationModal class="btn btn-primary" type="submit">Register</button>
      </form>      
    </div>
  </nav>
</template>

<style scoped>
  button{
    float: right;
    margin-right: 1em    
  }
</style>

<script>
import { mapGetters, mapState, mapActions } from 'vuex'

export default {
  computed: {
    ...mapState('context', [
      'profile'
    ]),
    ...mapGetters('context', [
      'isAuthenticated'
    ])
  },
  methods: {
    ...mapActions('context', [
      'logout'
    ]),
    onclick(evt){        
        evt.preventDefault();
    },
    showCartModal(){
      this.$bvModal.show('cartModal');
    }
  }
}
</script>
