<template>
  <div class="home">    
    <catalog/>
  </div>
</template>

<script>
// @ is an alias to /src
import Catalog from '../components/catalog.vue';

export default {
  name: "Home",
  components: {    
    Catalog
  },
};
</script>
