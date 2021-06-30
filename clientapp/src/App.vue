<template>
  <div id="app">
    <MainNavbar/>
    <main role="main" class="container mt-4">
      <router-view/>
    </main>
    <login-modal/>
    <cart-modal/>
    <registration-modal/>
  </div>
</template>

<style lang="scss">
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}

#nav {
  padding: 30px;

  a {
    font-weight: bold;
    color: #2c3e50;

    &.router-link-exact-active {
      color: #42b983;
    }
  }
}
</style>

<script>
import CartModal from './components/cart-modal.vue'
import LoginModal from './components/login-modal.vue'
import MainNavbar from './components/main-navbar'
import RegistrationModal from './components/registration-modal.vue'

export default {
  name: 'App',
  components:{
    MainNavbar,
    LoginModal,
    CartModal,
    RegistrationModal    
  }
}
</script>
